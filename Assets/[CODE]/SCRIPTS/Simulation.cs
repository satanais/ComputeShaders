using System;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    #region Members
    public int downSizeFactor = 3;
    public float brushSize = 0.05f;
    public float brushSizeStep = 0.01f;
    public float brushFlow = 10f;
    public float brushFlowStep = 0.25f;


    private DoubleBuffer _wallsBuffer;
    private DoubleBuffer _waterBuffer;
    private Vector2Int _screenSize;
    private Vector2Int _canvasSize;
    private Vector2Int _dispatchSize;
    private float _ratio;
    private bool _isInitialize = false;
    private Shader _editWallsShader;
    private Material _editWallMaterial;
    private Shader _CombineShader;
    private Material _CombineMaterial;
    private Shader _editWaterShader;
    private Material _editWaterMaterial;
    private ComputeShader _simulationComputeShader;

    #endregion

    #region Func

    private void Initialize()
    {
        if (!_isInitialize)
        {
            _simulationComputeShader = Resources.Load<ComputeShader>("Simulation");
            _screenSize.x = Screen.width;
            _screenSize.y = Screen.height;
            uint numThreadx, numThready, numThreadz = 0;
            _simulationComputeShader.GetKernelThreadGroupSizes(0, out numThreadx, out numThready, out numThreadz);
            _canvasSize.x = (_screenSize.x >> downSizeFactor).Snap(numThreadx);
            _canvasSize.y = (_screenSize.y >> downSizeFactor).Snap(numThready);
            _dispatchSize.x = _canvasSize.x/ (int)numThreadx;
            _dispatchSize.y = _canvasSize.y/(int)numThready;
            _ratio = (float)_screenSize.x / _screenSize.y;
            Shader.SetGlobalFloat("_Ratio", _ratio);
            _wallsBuffer = new DoubleBuffer(_canvasSize.x, _canvasSize.y, RenderTextureFormat.R8, FilterMode.Point, TextureWrapMode.Clamp, true);

            _waterBuffer = new DoubleBuffer(_canvasSize.x, _canvasSize.y, RenderTextureFormat.R8, FilterMode.Point, TextureWrapMode.Clamp, true);
        
            _editWallsShader = Resources.Load<Shader>("EditWallsShader");
            _editWallMaterial = new Material(_editWallsShader);
            _CombineShader = Resources.Load<Shader>("CombineShader");
            _CombineMaterial = new Material(_CombineShader);
            _editWaterShader = Resources.Load<Shader>("EditWaterShader");
            _editWaterMaterial = new Material(_editWaterShader);

            _isInitialize = true;

        }
    }


    private void UnInitialize()
    {
        if (_isInitialize)
        {
            _wallsBuffer.Release();
            _wallsBuffer = null;
            _waterBuffer.Release();
            _waterBuffer = null;
            _isInitialize = false;
        }
    }

    private void Reinitialize()
    {
        UnInitialize();
        Initialize();
    }
    private void EditData()
    {
        bool click = Input.GetMouseButton(0);
        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool rightclick = Input.GetMouseButton(1);

        Vector2 mousePosition = (Vector2)Input.mousePosition / _screenSize;
        Shader.SetGlobalVector("_MousePosition", mousePosition);

        if (Input.GetKey(KeyCode.UpArrow))
            brushSize += brushSizeStep * Time.deltaTime;



        if (Input.GetKey(KeyCode.DownArrow))
            brushSize -= brushSizeStep * Time.deltaTime;

        if (Input.GetKey(KeyCode.LeftArrow))
            brushFlow -= brushFlowStep * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow))
            brushFlow += brushFlowStep * Time.deltaTime;

        brushFlow = Mathf.Max(brushFlow, 0);
        brushSize = Mathf.Clamp(brushSize, 0.0f, 1.0f);

        Shader.SetGlobalFloat("brushFlow", brushFlow * Time.deltaTime);

        Shader.SetGlobalFloat("_BrushSize", brushSize);
        if (rightclick)
        {
            _editWaterMaterial.SetTexture("_inputWaterBuffer", _waterBuffer.Read);
            _editWaterMaterial.SetTexture("_inputWallsBuffer", _wallsBuffer.Read);

            Graphics.Blit(null, _waterBuffer.Write, _editWaterMaterial);
            _waterBuffer.Swap();

        }
        if (click)
        {

            _editWallMaterial.SetInt("_operator", shift ? -1 : 1);
            _editWallMaterial.SetTexture("_inputWallsBuffer", _wallsBuffer.Read);
            Graphics.Blit(null, _wallsBuffer.Write, _editWallMaterial);
            _wallsBuffer.Swap();

        }

    }


    private void UpdateSimulation()
    {

        _simulationComputeShader.SetTexture(0, "inputWaterTexture", _waterBuffer.Read);
        _simulationComputeShader.SetTexture(0, "outputWaterTexture", _waterBuffer.Write);
        _simulationComputeShader.Dispatch(0, _dispatchSize.x, _dispatchSize.y, 1);
        _waterBuffer.Swap();
    }
    #endregion



    #region Unity Func
    void OnEnable()
    {
        Initialize();
    }
    private void OnDisable()
    {
        UnInitialize();
    }

    void Update()
    {
        EditData();
        UpdateSimulation();

    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        _CombineMaterial.SetTexture("_WallsTexture", _wallsBuffer.Read);
        _CombineMaterial.SetTexture("_WaterTexture", _waterBuffer.Read);

        Graphics.Blit(null, destination, _CombineMaterial);

    }

    #endregion
}
