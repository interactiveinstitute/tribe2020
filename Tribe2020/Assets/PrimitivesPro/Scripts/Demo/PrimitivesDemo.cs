using System.Collections.Generic;
using UnityEngine;

internal class PrimitivesDemo : MonoBehaviour
{
    /// <summary>
    /// instance of this class
    /// </summary>
    public static PrimitivesDemo Instance
    {
        get { return instance; }
    }

    private static PrimitivesDemo instance;

    public float animTimeMax = 5.0f;

    private List<GameObject> buttons;

    private PrimitivesPro.GameObjects.BaseObject shapeMain;
    private PrimitivesPro.GameObjects.BaseObject shapeOld;
    private float animTimeout;
    private float[] shapeParamsStart;
    private float[] shapeParamsMax;
    private int shapeID;

    public Transform centralPosition = null;
    public Transform prevPosition = null;
    public Transform nextPosition = null;

    private float nextShowTimeout;
    private float nextShowTimeoutMax = 1;

    private bool textureToggle;
    private bool flatNormals;

    private void Start()
    {
        instance = this;
        buttons = new List<GameObject>(16);

        animTimeout = 0.0f;
        shapeParamsStart = new float[6];
        shapeParamsMax = new float[6];

        Vector3 pos = Vector3.zero;
        pos.z = -0.8f;
        pos.x = -3.1f;

        for (int i = 0; i < 20; i++)
        {
            CreateButton(i, pos);
            pos.x = pos.x + 0.73f;
        }

        CreateButton(20, new Vector3(10, 0, 5));
        CreateButton(21, new Vector3(10, 0, 3.8f));

        buttons[20].GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
        buttons[20].GetComponent<Renderer>().material.mainTexture = Resources.Load("sphereChecker") as Texture2D;

        buttons[21].GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Texture"));
        buttons[21].GetComponent<Renderer>().material.mainTexture = Resources.Load("faceNormalsIcon") as Texture2D;

        OnButtonHit(0);
    }

    /// <summary>
    /// event when user pressed the button
    /// </summary>
    /// <param name="id">id of the button</param>
    public void OnButtonHit(int id)
    {
        animTimeout = animTimeMax;

        // create buttons shapes
        PrimitivesPro.GameObjects.BaseObject shapeObject = null;

        switch (id)
        {
            case 0:
                shapeObject = PrimitivesPro.GameObjects.Triangle.Create(1f, 0);
                shapeParamsMax = new float[] { 4.0f, 4.0f, 0, 0, 0, 0};
                shapeParamsStart = new float[] { 1f, 1f, 0, 0, 0, 0};
                break;

            case 1:
                shapeObject = PrimitivesPro.GameObjects.PlaneObject.Create(1f, 1f, 1, 1);
                shapeParamsMax = new float[] {4.0f, 4.0f, 1, 1, 0, 0};
                shapeParamsStart = new float[] {1f, 1f, 1, 1, 0, 0};
                break;

            case 2:
                shapeObject = PrimitivesPro.GameObjects.Circle.Create(1.0f, 3);
                shapeParamsMax = new float[] {2.5f, 40, 0, 0, 0, 0};
                shapeParamsStart = new float[] {1.0f, 3, 0, 0, 0, 0};
                break;

            case 3:
                shapeObject = PrimitivesPro.GameObjects.Ellipse.Create(1.0f, 0.5f, 3);
                shapeParamsMax = new float[] {2.5f, 1.2f, 40, 0, 0, 0};
                shapeParamsStart = new float[] {1.0f, 0.5f, 3, 0, 0, 0};
                break;

            case 4:
                shapeObject = PrimitivesPro.GameObjects.Ring.Create(0.5f, 1.0f, 3);
                shapeParamsMax = new float[] {1.0f, 2.5f, 40, 0, 0, 0};
                shapeParamsStart = new float[] {0.5f, 1.0f, 3, 0, 0, 0};
                break;

            case 5:
                shapeObject = PrimitivesPro.GameObjects.Box.Create(1f, 1f, 1f, 1, 1, 1, false, null, PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {2.5f, 2.5f, 2.5f, 1, 1, 1};
                shapeParamsStart = new float[] {1f, 1f, 1f, 1, 1, 1};
                break;

            case 6:
                shapeObject = PrimitivesPro.GameObjects.Cylinder.Create(1f, 3, 3, 1, flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                              PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.25f, 4f, 40, 1, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 3, 3, 1, 0, 0, 0};
                break;

            case 7:
                shapeObject = PrimitivesPro.GameObjects.Cone.Create(1, 0, 0, 2, 3, 10, flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                          PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.25f, 0, 4f, 40, 10, 0, 0};
                shapeParamsStart = new float[] {1, 1, 2, 3, 10, 0, 0};
                break;

            case 8:
                shapeObject = PrimitivesPro.GameObjects.Sphere.Create(1f, 4, 0, 0, 0, flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                            PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {2.25f, 40, 0, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 4, 0, 0, 0, 0};
                break;

            case 9:
                shapeObject = PrimitivesPro.GameObjects.Ellipsoid.Create(1, 1, 1, 4, flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                               PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.25f, 2.45f, 2.5f, 40, 0, 0, 0};
                shapeParamsStart = new float[] {1, 1, 1, 4, 0, 0, 0};
                break;

            case 10:
                shapeObject = PrimitivesPro.GameObjects.Pyramid.Create(1, 1, 1, 1, 1, 1, false, PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {2.7f, 2.7f, 1.7f, 1, 1, 1, 0, 0, 0};
                shapeParamsStart = new float[] {1, 1, 1, 1, 1, 1, 0, 0, 0};
                break;

            case 11:
                shapeObject = PrimitivesPro.GameObjects.GeoSphere.Create(1f, 0, PrimitivesPro.Primitives.GeoSpherePrimitive.BaseType.Icosahedron,
                                               flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                               PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {2.45f, 4, 0, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 0, 0, 0, 0, 0};
                break;

            case 12:
                shapeObject = PrimitivesPro.GameObjects.Tube.Create(0.8f, 1f, 1f, 3, 1, 0.0f, false, flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                          PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {0.8f, 1.5f, 4f, 40, 0, 0, 0, 0};
                shapeParamsStart = new float[] {0.8f, 1f, 1f, 3, 0, 0, 0, 0};
                break;

            case 13:
                shapeObject = PrimitivesPro.GameObjects.Capsule.Create(1f, 1f, 4, 1, false, flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                             PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.2f, 4f, 40, 1, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 1f, 4, 1, 0, 0, 0};
                break;

            case 14:
                shapeObject = PrimitivesPro.GameObjects.RoundedCube.Create(1f, 1f, 1f, 1, 0.2f,
                                                 flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                 PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.6f, 1.6f, 1.6f, 20, 0.6f, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 1f, 1f, 1, 0.2f, 0, 0, 0};
                break;

            case 15:
                shapeObject = PrimitivesPro.GameObjects.Torus.Create(1f, 0.5f, 4, 4, 0, flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                           PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {1.6f, 0.8f, 40, 40, 0, 0, 0};
                shapeParamsStart = new float[] {1f, 0.5f, 4, 4, 0, 0, 0};
                break;

            case 16:
                shapeObject = PrimitivesPro.GameObjects.TorusKnot.Create(0.5f, 0.3f, 10, 4, 2, 3,
                                               flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                               PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] {1f, 0.5f, 120, 40, 2, 3, 0, 0, 0};
                shapeParamsStart = new float[] {0.5f, 0.3f, 10, 4, 2, 3, 0, 0, 0};
                break;

            case 17:
                shapeObject = PrimitivesPro.GameObjects.Arc.Create(1.0f, 1.0f, 1.0f, 1.0f, 10, PrimitivesPro.Primitives.PivotPosition.Botttom);
                ((PrimitivesPro.GameObjects.Arc)shapeObject).gizmo.gameObject.transform.localPosition = new Vector3(-1, -1, 0);
                shapeParamsMax = new float[] { 4.0f, 3.0f, 2.0f, 1.0f, 20, -1.0f };
                shapeParamsStart = new float[] { 0.5f, 0.5f, 0.1f, 0.5f, 0, 0.0f };
                break;

            case 18:
                shapeObject = PrimitivesPro.GameObjects.SphericalCone.Create(1, 20, 180, 
                                               flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                               PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[] { 2, 40, 20, 0, 0, 0 };
                shapeParamsStart = new float[] { 1, 20, 360, 0, 0, 0 };
                break;

            case 19:
                shapeObject = PrimitivesPro.GameObjects.SuperEllipsoid.Create(1, 1, 1, 20, 0.5f, 1.0f, 
                                               flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                               PrimitivesPro.Primitives.PivotPosition.Botttom);
                shapeParamsMax = new float[]   { 2,       2,    2, 20, 0.5f, 1.0f, 0, 0, 0 };
                shapeParamsStart = new float[] { 0.5f, 0.5f, 0.5f,  1, 0.0f, 0.0f, 0, 0, 0 };
                break;

            case 20:
                textureToggle = !textureToggle;
                break;

            case 21:
                flatNormals = !flatNormals;
                break;
        }

        if (shapeObject)
        {
            if (shapeOld)
            {
                Destroy(shapeOld.gameObject);
            }

            shapeOld = shapeMain;
            shapeMain = shapeObject;

            shapeMain.gameObject.GetComponent<Renderer>().material = new Material(GetSpecularShader());
            shapeMain.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 180.0f/255f, 180f/255f));
            shapeMain.gameObject.GetComponent<Renderer>().material.SetColor("_SpecColor", Color.white);
            shapeMain.gameObject.transform.position = prevPosition.position;

            nextShowTimeout = nextShowTimeoutMax;

            shapeID = id;
        }

        if (textureToggle)
        {
            shapeMain.GetComponent<MeshRenderer>().sharedMaterial = Resources.Load("Checker") as Material;
        }
        else
        {
            shapeMain.GetComponent<MeshRenderer>().sharedMaterial = new Material(GetSpecularShader());
            shapeMain.gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 180.0f/255f, 180f/255f));
            shapeMain.gameObject.GetComponent<Renderer>().material.SetColor("_SpecColor", Color.white);
        }
    }

    Shader GetSpecularShader()
    {
        return Shader.Find("Specular");
    }

    /// <summary>
    /// event when user hover over the button
    /// </summary>
    /// <param name="id"></param>
    public void OnButtonHover(int id, bool start)
    {
        var transform = buttons[id].transform;
        var material = buttons[id].GetComponent<MeshRenderer>().sharedMaterial;

        if (start)
        {
            material.color = new Color(1, 0, 0);
            transform.position -= new Vector3(0, 0.28f, 0.0f);
        }
        else
        {
            material.color = new Color(1, 1, 1);
            transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
        }
    }

    private Texture2D GetTexture(Texture2D tex)
    {
//        return tex;
        var newTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
        newTex.SetPixels32(tex.GetPixels32());

//        var pixels = newTex.GetPixels32();
//
//        for (int i = 0; i < pixels.Length; i++)
//        {
//            pixels[i].a = 1;
//        }
//
//        newTex.SetPixels32(pixels);

        newTex.Apply();
        return newTex;
    }

    /// <summary>
    /// create plane button
    /// </summary>
    /// <param name="id">id of the button</param>
    /// <param name="position"></param>
    private void CreateButton(int id, Vector3 position)
    {
        var plane = PrimitivesPro.GameObjects.PlaneObject.Create(0.67f, 0.67f, 2, 2);
        plane.gameObject.GetComponent<MeshRenderer>().sharedMaterial.color = new Color(1, 1, 1, 1);

        var collider = plane.gameObject.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        plane.gameObject.transform.position = position;

        var trigger = plane.gameObject.AddComponent<ButtonTrigger>();
        trigger.ID = id;

        buttons.Add(plane.gameObject);

        switch (id)
        {
            case 0:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/triangle") as Texture2D);;
                break;

            case 1:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/plane") as Texture2D);;
                break;

            case 2:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/circle") as Texture2D);;
                break;

            case 3:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/ellipse") as Texture2D);;
                break;

            case 4:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/ring") as Texture2D);;
                break;

            case 5:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/box") as Texture2D);;
                break;

            case 6:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/cylinder") as Texture2D);;
                break;

            case 7:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/cone") as Texture2D);;
                break;

            case 8:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/sphere") as Texture2D);;
                break;

            case 9:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/ellipsoid") as Texture2D);;
                break;

            case 10:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/pyramid") as Texture2D);;
                break;

            case 11:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/geosphere") as Texture2D);;
                break;

            case 12:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/tube") as Texture2D);;
                break;

            case 13:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/capsule") as Texture2D);;
                break;

            case 14:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/roundedBox") as Texture2D);;
                break;

            case 15:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/torus") as Texture2D);;
                break;

            case 16:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/torusKnot") as Texture2D);;
                break;

            case 17:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/arc") as Texture2D);;
                break;

            case 18:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/sphericalcone") as Texture2D);;
                break;

            case 19:
                plane.gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Unlit/Transparent"));
                plane.gameObject.GetComponent<Renderer>().material.mainTexture = GetTexture(Resources.Load("icons/superellipsoid") as Texture2D);;
                break;
        }

        if (plane)
        {
            plane.gameObject.transform.position = position + new Vector3(0, 0.1f, 0);
        }
    }

    private void Update()
    {
        if (nextShowTimeout > 0)
        {
            var t = 1.0f - nextShowTimeout/nextShowTimeoutMax;

            if (shapeOld)
            {
                var newPos = Vector3.Lerp(shapeOld.gameObject.transform.position, nextPosition.position, t);
                shapeOld.gameObject.transform.position = newPos;
            }

            if (shapeMain)
            {
                var newPos = Vector3.Lerp(shapeMain.gameObject.transform.position, centralPosition.position, t);
                shapeMain.gameObject.transform.position = newPos;
            }

            nextShowTimeout -= Time.deltaTime;

            if (shapeOld != null && nextShowTimeout <= 0)
            {
                Object.Destroy(shapeOld.gameObject);
                shapeOld = null;
            }
        }

        if (shapeMain != null)
        {
            animTimeout -= Time.deltaTime;

            shapeMain.gameObject.transform.rotation = Quaternion.Euler(40, 0, 0);

            var rotation = Quaternion.Euler(0, 360*animTimeout/animTimeMax, 0);

            shapeMain.gameObject.transform.rotation *= rotation;

            if (animTimeout > 0)
            {
                var shapeParams = new float[6];

                float t = 1.0f - animTimeout/animTimeMax;

                for (int i = 0; i < 6; i++)
                {
                    shapeParams[i] = shapeParamsStart[i]*(1 - t) + shapeParamsMax[i]*t;
                }

                switch (shapeID)
                {
                    case 0:
                        ((PrimitivesPro.GameObjects.Triangle)shapeMain).GenerateGeometry(shapeParams[0], (int)shapeParams[1]);
                        break;

                    case 1:
                        ((PrimitivesPro.GameObjects.PlaneObject) shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], 1, 1);
                        break;

                    case 2:
                        ((PrimitivesPro.GameObjects.Circle)shapeMain).GenerateGeometry(shapeParams[0], (int)shapeParams[1]);
                        break;

                    case 3:
                        ((PrimitivesPro.GameObjects.Ellipse)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int)shapeParams[2]);
                        break;

                    case 4:
                        ((PrimitivesPro.GameObjects.Ring)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int)shapeParams[2]);
                        break;

                    case 5:
                        ((PrimitivesPro.GameObjects.Box)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2], 1, 1, 1,
                                                           false, null, PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 6:
                        ((PrimitivesPro.GameObjects.Cylinder)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int)shapeParams[2], 1,
                                                                flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                                PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 7:
                        ((PrimitivesPro.GameObjects.Cone)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], 0, shapeParams[2],
                                                            (int) shapeParams[3], (int) shapeParams[4],
                                                            flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                            PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 8:
                        ((PrimitivesPro.GameObjects.Sphere)shapeMain).GenerateGeometry(shapeParams[0], (int)shapeParams[1], 0.0f, 0.0f, 0.0f,
                                                              flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                              PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 9:
                        ((PrimitivesPro.GameObjects.Ellipsoid)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2],
                                                                 (int) shapeParams[3],
                                                                 flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                                 PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 10:
                        ((PrimitivesPro.GameObjects.Pyramid)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2], 1, 1, 1,
                                                               false, PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 11:
                        ((PrimitivesPro.GameObjects.GeoSphere)shapeMain).GenerateGeometry(shapeParams[0], (int)shapeParams[1],
                                                                 PrimitivesPro.Primitives.GeoSpherePrimitive.BaseType.
                                                                     Icosahedron,
                                                                 flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                                 PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 12:
                        ((PrimitivesPro.GameObjects.Tube)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2],
                                                            (int) shapeParams[3], (int) shapeParams[4],
                                                            (float) shapeParams[4], false,
                                                            flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                            PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 13:
                        ((PrimitivesPro.GameObjects.Capsule)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int)shapeParams[2],
                                                               (int) shapeParams[3], false,
                                                               flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                               PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 14:
                        ((PrimitivesPro.GameObjects.RoundedCube)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2],
                                                                   (int) shapeParams[3], shapeParams[4],
                                                                   flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                                   PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 15:
                        ((PrimitivesPro.GameObjects.Torus)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int)shapeParams[2],
                                                             (int) shapeParams[3], 0,
                                                             flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                             PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 16:
                        ((PrimitivesPro.GameObjects.TorusKnot)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], (int)shapeParams[2],
                                                                 (int) shapeParams[3], 3, 2,
                                                                 flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                                 PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 17:
                        ((PrimitivesPro.GameObjects.Arc)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2], shapeParams[3], (int)shapeParams[4],
                                                                 PrimitivesPro.Primitives.PivotPosition.Center);
                        ((PrimitivesPro.GameObjects.Arc)shapeMain).gizmo.gameObject.transform.localPosition = new Vector3(shapeParams[5],
                            shapeParams[5], 0.0f);
                        break;

                    case 18:
                        ((PrimitivesPro.GameObjects.SphericalCone)shapeMain).GenerateGeometry(shapeParams[0], (int)shapeParams[1], shapeParams[2], 
                                                                 flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                                 PrimitivesPro.Primitives.PivotPosition.Center);
                        break;

                    case 19:
                        ((PrimitivesPro.GameObjects.SuperEllipsoid)shapeMain).GenerateGeometry(shapeParams[0], shapeParams[1], shapeParams[2], (int)shapeParams[3], shapeParams[4], shapeParams[5],
                                                                 flatNormals ? PrimitivesPro.Primitives.NormalsType.Face : PrimitivesPro.Primitives.NormalsType.Vertex,
                                                                 PrimitivesPro.Primitives.PivotPosition.Center);
                        break;
                }
            }
        }
    }

    private void OnGUI()
    {
        var style = new GUIStyle();
        style.fontSize = 20;
        style.fontStyle = FontStyle.Bold;
    }
}
