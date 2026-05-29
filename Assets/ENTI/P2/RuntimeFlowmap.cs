using UnityEngine;

public class RuntimeFlowmap : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Material simulationMaterial;
    public Material waterMaterial;
    public Renderer waterRenderer;

    [Header("Ajustes de Simulación")]
    public int resolution = 512;

    [Header("Player")]
    public float playerRadius = 5f;

    [Range(0.01f, 0.95f)]
    public float playerHardness = 0.3f;

    [Header("Corriente base del agua")]
    public Vector2 baseFlowDirection = new Vector2(1f, 0.25f);

    [Range(0f, 1f)]
    public float baseFlowStrength = 1f;

    [Header("Desvanecimiento del rastro")]
    [Range(0f, 0.2f)]
    public float dissipation = 0.025f;

    [Header("Corrección UV del Plane")]
    public bool invertU = false;
    public bool invertV = false;
    

    private RenderTexture rtA;
    private RenderTexture rtB;
    private bool isUsingA = true;

    private Vector2 previousPlayerUV;
    private bool hasPreviousUV = false;

    private MaterialPropertyBlock propertyBlock;

    void OnEnable()
    {
        if (waterRenderer == null)
        {
            waterRenderer = GetComponent<Renderer>();
        }

        propertyBlock = new MaterialPropertyBlock();

        CreateRTs();
        ClearRTs();

        if (player != null && waterRenderer != null)
        {
            previousPlayerUV = WorldToPlaneUV(player.position);
            hasPreviousUV = true;
        }
    }

    void OnDisable()
    {
        ReleaseRTs();
    }

    void CreateRTs()
    {
        ReleaseRTs();

        rtA = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf);
        rtA.wrapMode = TextureWrapMode.Clamp;
        rtA.filterMode = FilterMode.Bilinear;
        rtA.Create();

        rtB = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf);
        rtB.wrapMode = TextureWrapMode.Clamp;
        rtB.filterMode = FilterMode.Bilinear;
        rtB.Create();
    }

    void ReleaseRTs()
    {
        if (rtA != null)
        {
            rtA.Release();
            Destroy(rtA);
            rtA = null;
        }

        if (rtB != null)
        {
            rtB.Release();
            Destroy(rtB);
            rtB = null;
        }
    }

    void ClearRTs()
    {
        if (rtA == null || rtB == null)
            return;

        Vector2 baseDir = baseFlowDirection.normalized;

        Vector2 encodedBase = (-baseDir * baseFlowStrength) * 0.5f + Vector2.one * 0.5f;
        Color clearColor = new Color(encodedBase.x, encodedBase.y, 0f, 1f);

        RenderTexture previous = RenderTexture.active;

        RenderTexture.active = rtA;
        GL.Clear(true, true, clearColor);

        RenderTexture.active = rtB;
        GL.Clear(true, true, clearColor);

        RenderTexture.active = previous;
    }

    void LateUpdate()
    {
        if (player == null || simulationMaterial == null || waterRenderer == null)
            return;

        if (rtA == null || rtB == null)
        {
            CreateRTs();
            ClearRTs();
        }

        Vector2 currentPlayerUV = WorldToPlaneUV(player.position);

        Vector2 velocityUV = Vector2.zero;

        if (hasPreviousUV)
        {
            Vector2 deltaUV = currentPlayerUV - previousPlayerUV;

            if (deltaUV.sqrMagnitude > 0.000001f)
            {
                velocityUV = deltaUV.normalized;
            }
        }

        previousPlayerUV = currentPlayerUV;
        hasPreviousUV = true;

        RenderTexture readTex = isUsingA ? rtA : rtB;
        RenderTexture writeTex = isUsingA ? rtB : rtA;

        Vector2 waterWorldSize = GetPlaneWorldSize();

        simulationMaterial.SetTexture("_PreviousFrame", readTex);

        simulationMaterial.SetVector("_PlayerUV", new Vector4(currentPlayerUV.x, currentPlayerUV.y, 0f, 0f));
        simulationMaterial.SetVector("_VelocityUV", new Vector4(velocityUV.x, velocityUV.y, 0f, 0f));

        simulationMaterial.SetVector("_SimulationSize", new Vector4(waterWorldSize.x, 0f, waterWorldSize.y, 0f));

        simulationMaterial.SetFloat("_PlayerRadius", playerRadius);
        simulationMaterial.SetFloat("_PlayerHardness", playerHardness);

        Vector2 baseDir = baseFlowDirection.normalized;
        simulationMaterial.SetVector("_BaseFlowDirection", new Vector4(baseDir.x, baseDir.y, 0f, 0f));
        simulationMaterial.SetFloat("_BaseFlowStrength", baseFlowStrength);
        simulationMaterial.SetFloat("_Dissipation", dissipation);

        Graphics.Blit(readTex, writeTex, simulationMaterial, 0);

        if (waterMaterial != null)
        {
            waterMaterial.SetTexture("_Flowmap", writeTex);
        }

        waterRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetTexture("_Flowmap", writeTex);
        waterRenderer.SetPropertyBlock(propertyBlock);

        isUsingA = !isUsingA;
    }

    Vector2 WorldToPlaneUV(Vector3 worldPosition)
    {
        Transform planeTransform = waterRenderer.transform;

        Vector3 localPosition = planeTransform.InverseTransformPoint(worldPosition);

        MeshFilter meshFilter = waterRenderer.GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            return Vector2.zero;
        }

        Bounds meshBounds = meshFilter.sharedMesh.bounds;

        float u = Mathf.InverseLerp(meshBounds.min.x, meshBounds.max.x, localPosition.x);
        float v = Mathf.InverseLerp(meshBounds.min.z, meshBounds.max.z, localPosition.z);

        Vector2 uv = new Vector2(u, v);


        if (invertU)
        {
            uv.x = 1f - uv.x;
        }

        if (invertV)
        {
            uv.y = 1f - uv.y;
        }

        return uv;
    }

    Vector2 GetPlaneWorldSize()
    {
        MeshFilter meshFilter = waterRenderer.GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            return Vector2.one;
        }

        Bounds meshBounds = meshFilter.sharedMesh.bounds;
        Vector3 lossyScale = waterRenderer.transform.lossyScale;

        float width = Mathf.Abs(meshBounds.size.x * lossyScale.x);
        float depth = Mathf.Abs(meshBounds.size.z * lossyScale.z);

        return new Vector2(width, depth);
    }
}