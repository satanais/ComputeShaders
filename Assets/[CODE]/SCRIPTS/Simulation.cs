using System;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    #region Members
    public int downSizeFactor = 3;
    public float brushSize = 0.05f;
    public float brushSizeStep = 0.01f;


    private RenderTexture _wallsTexture;
    private Vector2Int _screenSize;
    private Vector2Int _canvasSize;
    private float _ratio;
    private bool _isInitialize = false;
    private Shader _editWallsShader;
    private Material _editWallMaterial;
    private Shader _CombineShader;
    private Material _CombineMaterial;

    #endregion

    #region Func

    private void Initialize()
    {
        if (!_isInitialize)
        {
            _screenSize.x = Screen.width;
            _screenSize.y = Screen.height;
            _canvasSize.x = _screenSize.x >> downSizeFactor;
            _canvasSize.y = _screenSize.y >> downSizeFactor;
            _ratio = (float)_screenSize.x / _screenSize.y;
            Shader.SetGlobalFloat("_Ratio", _ratio);
            _wallsTexture = new RenderTexture(_canvasSize.x, _canvasSize.y, -1, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            _wallsTexture.filterMode = FilterMode.Point;
            _wallsTexture.wrapMode = TextureWrapMode.Clamp;
            _wallsTexture.enableRandomWrite = true;
            _wallsTexture.Create();
            _editWallsShader = Resources.Load<Shader>("EditWallsShader");
            _editWallMaterial = new Material(_editWallsShader);
            _CombineShader = Resources.Load<Shader>("CombineShader");
            _CombineMaterial = new Material(_CombineShader);
            _CombineMaterial.SetTexture("_WallsTetxure", _wallsTexture);

            _isInitialize = true;

        }
    }

    private void UnInitialize()
    {
        if (_isInitialize)
        {
            _wallsTexture.Release();
            _wallsTexture = null;
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

        Vector2 mousePosition = (Vector2)Input.mousePosition / _screenSize;
        Shader.SetGlobalVector("_MousePosition", mousePosition);

        if (Input.GetKey(KeyCode.UpArrow))
            brushSize += brushSizeStep * Time.deltaTime;


        if (Input.GetKey(KeyCode.DownArrow))
            brushSize -= brushSizeStep * Time.deltaTime;


        brushSize = Mathf.Clamp(brushSize, 0.0f, 1.0f);

        Shader.SetGlobalFloat("_BrushSize", brushSize);
        if (click)
            Graphics.Blit(null, _wallsTexture, _editWallMaterial);

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

    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(null, destination, _CombineMaterial);

    }

    #endregion
}
