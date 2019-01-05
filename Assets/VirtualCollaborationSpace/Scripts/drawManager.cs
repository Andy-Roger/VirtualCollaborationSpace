using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class drawManager : MonoBehaviour
{
    private Camera cam;
    private PhotonView photonView;
    public Renderer whiteboardRenderer;
    private Texture2D tex;
    private Vector2 pixelUV;

    public Hand hand1;
    public Hand hand2;

    public Camera nonVRCamera;

    private List<Vector2> points = new List<Vector2>();

    private Color sampledHitColor = Color.black;

    void Start()
    {
        //HACK : on trigger up make zero, then in update dont draw if zero
        points.Add(Vector2.zero);
        points.Add(Vector2.zero);

        cam = nonVRCamera;
        photonView = GetComponent<PhotonView>();
        tex = whiteboardRenderer.material.mainTexture as Texture2D;
    }

    void Update()
    {
        //if user is on vr
        if (clientTypeManager.isVR)
        {
            if (hand1 != null && hand1.controller.GetHairTrigger())
            {
                raycastDrawTool(new Ray(hand1.transform.position, hand1.transform.forward));
            }
            if (hand2 != null && hand2.controller.GetHairTrigger())
            {
                raycastDrawTool(new Ray(hand2.transform.position, hand2.transform.forward));
            }

            if (hand1 != null && hand1.controller.GetHairTriggerUp())
            {
                points[0] = Vector2.zero;
                points[1] = Vector2.zero;
            }
            if (hand2 != null && hand2.controller.GetHairTriggerUp())
            {
                points[0] = Vector2.zero;
                points[1] = Vector2.zero;
            }
        }
        else
        {
            //if user is on pc
            if(Input.GetMouseButtonUp(0))
            {
                points[0] = Vector2.zero;
                points[1] = Vector2.zero;
            }
            if (Input.GetMouseButton(0))
            {
                raycastDrawTool(cam.ScreenPointToRay(Input.mousePosition));
            }
        }
    }

    void raycastDrawTool(Ray ray)
    {
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
        {
            return;
        }

        if(hit.collider.gameObject != whiteboardRenderer.gameObject)
        {
            points[0] = Vector2.zero;
            points[1] = Vector2.zero;
            return;
        }

        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (whiteboardRenderer == null || whiteboardRenderer.sharedMaterial == null || whiteboardRenderer.sharedMaterial.mainTexture == null || meshCollider == null)
        {
            return;
        }

        pixelUV = hit.textureCoord;
        pixelUV.x *= tex.width;
        pixelUV.y *= tex.height;

        points[0] = points[1];
        points[1] = pixelUV;

        if(points[0] != Vector2.zero)
        {
            line((int)points[0].x, (int)points[0].y, (int)points[1].x, (int)points[1].y, sampledHitColor.r, sampledHitColor.g, sampledHitColor.b);
            photonView.RPC("recieveRemotePixels", PhotonTargets.AllBuffered, (int)points[0].x, (int)points[0].y, (int)points[1].x, (int)points[1].y, sampledHitColor.r, sampledHitColor.g, sampledHitColor.b);
        }
    }

    public void sampleColor()
    {
        if (clientTypeManager.isVR)
        {
            if (hand1.controller.GetHairTrigger())
            {
                sampleColorWithRaycastHandler(new Ray(hand1.transform.position, hand1.transform.forward));
            }
            if (hand2.controller.GetHairTrigger())
            {
                sampleColorWithRaycastHandler(new Ray(hand2.transform.position, hand2.transform.forward));
            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                sampleColorWithRaycastHandler(cam.ScreenPointToRay(Input.mousePosition));
            }
        }
    }

    void sampleColorWithRaycastHandler(Ray ray)
    {
        //raycast from here, sample tex, set color
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit))
        {
            return;
        }

        Renderer rend = hit.transform.GetComponent<Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
        {
            sampledHitColor = rend.material.color;
        }
        else
        {
            Texture2D hitTex = rend.material.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= hitTex.width;
            pixelUV.y *= hitTex.height;

            sampledHitColor = hitTex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
        }
    }

    //pass all four coords and color
    [PunRPC]
    void recieveRemotePixels(int point0x, int point0y, int point1x, int point1y, float r, float g, float b)
    {
        line(point0x, point0y, point1x, point1y, r, g, b);
    }

    void line(int x, int y, int x2, int y2, float r, float g, float b)
    {
        int w = x2 - x;
        int h = y2 - y;
        int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
        if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
        if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
        if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
        int longest = Mathf.Abs(w);
        int shortest = Mathf.Abs(h);
        if (!(longest > shortest))
        {
            longest = Mathf.Abs(h);
            shortest = Mathf.Abs(w);
            if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
            dx2 = 0;
        }
        int numerator = longest >> 1;
        for (int i = 0; i <= longest; i++)
        {
            circle(x, y, 2, new Color(r, g, b, 1));

            numerator += shortest;
            if (!(numerator < longest))
            {
                numerator -= longest;
                x += dx1;
                y += dy1;
            }
            else
            {
                x += dx2;
                y += dy2;
            }
        }
    }

    //make circle of pxls around line pxls
    private void circle(int cx, int cy, int r, Color col)
    {
        int x, y, px, nx, py, ny, d;
        Color32[] tempArray = tex.GetPixels32(0);

        for (x = 0; x <= r; x++)
        {
            d = (int)Mathf.Ceil(Mathf.Sqrt(r * r - x * x));
            for (y = 0; y <= d; y++)
            {
                px = cx + x;
                nx = cx - x;
                py = cy + y;
                ny = cy - y;

                tempArray[py * tex.width + px] = col;
                tempArray[py * tex.width + nx] = col;
                tempArray[ny * tex.width + px] = col;
                tempArray[ny * tex.width + nx] = col;
            }
        }
        tex.SetPixels32(tempArray);
        tex.Apply();
    }
}
