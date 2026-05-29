using UnityEngine;

[ExecuteAlways]
public class RuntimeFlowmap : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Material simulationMaterial;
    public Material waterMaterial;

    [Header("Ajustes de Simulación")]
    public int resolution = 512;
    public float simulationSize = 396f;
    public float playerRadius = 50f;
    [Range(0f, 1f)]
    public float playerHardness = 0.3f;

    [Header("Depuración (opcional)")]
    public bool showFlowOnThisRenderer = false;

    private RenderTexture rtA;
    private RenderTexture rtB;
    private bool isUsingA = true;

    private Vector3 previousPosition;

    void OnEnable()
    {
        CreateRTs();
        ClearRTs();

        if (player != null)
            previousPosition = player.position;
    }

    void OnDisable()
    {
        ReleaseRTs();
    }

    void CreateRTs()
    {
        if (rtA == null)
        {
            rtA = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf);
            rtA.wrapMode = TextureWrapMode.Clamp;
            rtA.filterMode = FilterMode.Bilinear;
            rtA.Create();
        }
        if (rtB == null)
        {
            rtB = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGBHalf);
            rtB.wrapMode = TextureWrapMode.Clamp;
            rtB.filterMode = FilterMode.Bilinear;
            rtB.Create();
        }
    }

    void ReleaseRTs()
    {
        if (rtA != null) { rtA.Release(); DestroyImmediate(rtA); rtA = null; }
        if (rtB != null) { rtB.Release(); DestroyImmediate(rtB); rtB = null; }
    }

    void ClearRTs()
    {
        var prev = RenderTexture.active;
        RenderTexture.active = rtA;
        GL.Clear(true, true, new Color(0.5f, 0.5f, 0f, 1f));
        RenderTexture.active = rtB;
        GL.Clear(true, true, new Color(0.5f, 0.5f, 0f, 1f));
        RenderTexture.active = prev;
    }

    void Update()
    {
        if (player == null || simulationMaterial == null) return;

        Vector3 currentVelocity = Application.isPlaying && Time.deltaTime > 0f
            ? (player.position - previousPosition) / Time.deltaTime
            : Vector3.zero;
        previousPosition = player.position;

        simulationMaterial.SetVector("_Position", player.position);
        simulationMaterial.SetVector("_Velocity", currentVelocity);
        simulationMaterial.SetVector("_Simulation_Centre", transform.position);
        simulationMaterial.SetVector("_SimulationSize", new Vector3(simulationSize, 0f, simulationSize));
        simulationMaterial.SetFloat("_PlayerRadius", playerRadius);
        simulationMaterial.SetFloat("_PlayerHardness", playerHardness);

        RenderTexture readTex = isUsingA ? rtA : rtB;
        RenderTexture writeTex = isUsingA ? rtB : rtA;

        simulationMaterial.SetTexture("_PreviousFrame", readTex);
        Graphics.Blit(readTex, writeTex, simulationMaterial);

        if (waterMaterial != null)
            waterMaterial.SetTexture("_Flowmap", writeTex);

        if (showFlowOnThisRenderer)
        {
            var r = GetComponent<Renderer>();
            if (r != null && r.sharedMaterial != null)
                r.sharedMaterial.mainTexture = writeTex;
        }

        isUsingA = !isUsingA;
    }
}
