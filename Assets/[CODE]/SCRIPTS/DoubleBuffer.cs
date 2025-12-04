using UnityEngine;
using UnityEngine.UIElements;

public class DoubleBuffer
{
    private RenderTexture[] _buffers;
    private int _readID = 0;
    private int _writeID = 1;

    public RenderTexture Read => _buffers[_readID];

    public RenderTexture Write => _buffers[_writeID];

    public DoubleBuffer(int width, int height, RenderTextureFormat format, FilterMode mode, TextureWrapMode wrap, bool enableRandomWrite)
    {
        _buffers = new RenderTexture[2];
        for (int i = 0; i<2; i++)
        {
            _buffers[i] = new RenderTexture(width, height, -1, format, RenderTextureReadWrite.Linear);
            _buffers[i].filterMode = mode;
            _buffers[i].wrapMode = wrap;
            _buffers[i].enableRandomWrite = enableRandomWrite;
            _buffers[i].Create();
        }
    }

    public void Swap()
    {
        (_readID, _writeID) = (_writeID, _readID);
    }

    public void Release()
    {
        for (int i = 0; i < 2; i++)
        {
            _buffers[i].Release();
            _buffers[i] = null;
        }

        _buffers = null;

    }
}
