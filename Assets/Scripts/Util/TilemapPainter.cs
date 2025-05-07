using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapPainter : MonoBehaviour
{
    private Tilemap targetTilemap;
    private Vector3Int startPosition;

    [SerializeField] private Vector2Int size;
    [SerializeField] private PatternType patternType;
    [SerializeField] private TileBase[] patternWhiteTiles = new TileBase[4]; // A1, A2, A3, A4
    [SerializeField] private TileBase[] patternGrayTiles = new TileBase[4]; // B1, B2, B3, B4
    public PatternType PatternType => patternType;
    public TileBase[] PatternGrayTiles => patternGrayTiles; // B1, B2, B3, B4
    public TileBase[] PatternWhiteTiles => patternWhiteTiles; // A1, A2, A3, A4

    private void Init(Tilemap tilemap, Vector3Int start, Vector2Int size, PatternType type)
    {
        targetTilemap = tilemap;
        startPosition = start;
        this.size = size;
        patternType = type;
    }

    // 타일을 자동으로 배치하는 함수
    public void PlaceTiles(Tilemap tilemap, Vector3Int start, Vector2Int size, PatternType type, bool IsCenter = true)
    {
        Init(tilemap, start, size, type);

        TileBase[] pattern = patternType == PatternType.White ? 
                            patternWhiteTiles : patternGrayTiles;
        if (targetTilemap == null) return;

        if (IsCenter)
        {
            CenterPlace(pattern);
        }
        else
        {
            LeftBottomPlace(pattern);
        }

        Debug.Log("타일 배치 완료!");
    }

    private void CenterPlace(TileBase[] pattern)
    {
        int offsetX = size.x / 2;
        int offsetY = size.y / 2;

        // 지정된 가로, 세로 크기만큼 타일 배치
        for (int x = -offsetX; x < size.x - offsetX; x++)
        {
            for (int y = -offsetX; y < size.y - offsetY; y++)
            {
                int patternIndex = GetPatternIndex(x + offsetX, y + offsetY);
                Vector3Int pos = startPosition + Vector3Int.right * x + Vector3Int.up * y;
                targetTilemap.SetTile(pos, pattern[patternIndex]);
            }
        }
    }

    private void LeftBottomPlace(TileBase[] pattern)
    {
        // 지정된 가로, 세로 크기만큼 타일 배치
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int patternIndex = GetPatternIndex(x, y);
                Vector3Int pos = startPosition + Vector3Int.right * x + Vector3Int.up * y;
                targetTilemap.SetTile(pos, pattern[patternIndex]);
            }
        }
    }

    // 타일의 배치 패턴을 결정하는 함수 (가로/세로 순서에 맞게 인덱스를 설정)
    private int GetPatternIndex(int x, int y)
    {
        // y를 기준으로 위아래 반전
        bool isTop = (y % 2 == 1);
        bool isRight = (x % 2 == 1);

        if (isTop && !isRight) return 0; // A1 (좌상단)
        if (isTop && isRight) return 1;  // A2 (우상단)
        if (!isTop && !isRight) return 2;  // A3 (좌하단)
        if (!isTop && isRight) return 3;   // A4 (우하단)
        return 0;
    }
}
