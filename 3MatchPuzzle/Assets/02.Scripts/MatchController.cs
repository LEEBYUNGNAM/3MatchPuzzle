using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchController : MonoBehaviour
{
    float swapTime = .5f;

    public Color[] colors;

    // 맵타일
    public GameObject[] tiles;

    // 잼프리팹
    public GameObject gem;
    List<GameObject> newGems;

    // 처음 클릭되는 Gem
    GameObject firGem;
    GemController firGemController;
    // 두번째 클릭되는 Gem
    GameObject secGem;
    GemController secGemController;

    RaycastHit2D hit;
    Ray ray;

    // firGem 주변의 GemList
    Collider2D[] candidates;

    // 삭제할 Gem List
    List<GemController> destroyGems;

    // 삭제를 했는가 안했는가의 판단 변수
    bool isDestory = false;

    // 같은색 Gem 저장
    List<GemController> candidatesEqualsColors;

    // 삭제할 Gem 이 늘어났는지 체크
    int destroyGemsCount = 0;

    float swapSpeed = 1.5f;

    // 파괴된 Gem 중 가장아래의 Gem
    List<GemController> destroyedGemsBottoms;

    public GameObject[] zenTiles;
    public Sprite top1Img;
    public Sprite top2Img;

    // Gem 번호
    int gemNum = 0;

    bool isDownDone = false;

    public GameObject top;

    void Start()
    {
        InitializeGem();
    }

    void Update()
    {
        if (Application.platform.Equals(RuntimePlatform.Android))
        {
            if(Input.touchCount > 0)
            {
                // 두번째Gem 선택
                if (Input.GetTouch(0).phase.Equals(TouchPhase.Ended) && firGem && !secGem)
                {
                    ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

                    if (hit.collider)
                    {
                        if (hit.collider.gameObject.name.Equals(firGem.name) || !IsFirGemAround(firGem.transform.position))
                        {
                            firGem = null;
                            return;
                        }
                        secGem = hit.collider.gameObject;
                        secGemController = secGem.GetComponent<GemController>();
                    }
                }

                // 첫번째Gem 선택
                if (Input.GetTouch(0).phase.Equals(TouchPhase.Began) && !firGem && !secGem)
                {
                    ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                    hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

                    if (hit.collider)
                    {
                        firGem = hit.collider.gameObject;
                        firGemController = firGem.GetComponent<GemController>();
                    }
                }
            }
        }
        else
        {
            // 두번째Gem 선택
            if (Input.GetMouseButtonDown(0) && firGem && !secGem)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

                if (hit.collider)
                {
                    if (hit.collider.gameObject.name.Equals(firGem.name) || !IsFirGemAround(firGem.transform.position))
                    {
                        firGem = null;
                        return;
                    }
                    secGem = hit.collider.gameObject;
                    secGemController = secGem.GetComponent<GemController>();
                }
            }

            // 첫번째Gem 선택
            if (Input.GetMouseButtonDown(0) && !firGem && !secGem)
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity);

                if (hit.collider)
                {
                    firGem = hit.collider.gameObject;
                    firGemController = firGem.GetComponent<GemController>();
                }
            }
        }

        // Gem이 둘다 선택 되었을시
        if (firGem && secGem)
        {
            Initialization();
        }
    }

    // firGem 주변의 Gem을 움직이는지 판별
    bool IsFirGemAround(Vector2 firPos)
    {
        candidates = Physics2D.OverlapCircleAll(firPos, 0.93f);
        foreach (Collider2D candidate in candidates)
        {
            if (candidate.name.Equals(hit.collider.name))
            {
                return true;
            }
        }

        return false;
    }

    // 위치변경후 초기화
    void Initialization()
    {
        StartCoroutine(WaitSwapTime());
        firGem = null;
        secGem = null;
    }

    // 컬러 랜덤 설정
    Color GetRandomColor()
    {
        int index = Random.Range(0, colors.Length);
        return colors[index];
    }

    // 초기 Gem생성
    void InitializeGem()
    {
        int[] arr = Enumerable.Range(0, tiles.Length).ToArray();
        for (int i = 0; i < tiles.Length; ++i)
        {
            int index = Random.Range(i, tiles.Length);
            int tmp = arr[index];
            arr[index] = arr[i];
            arr[i] = tmp;
        }

        for (int i = 0; i < 5; ++i)
        {
            GameObject newTop = Instantiate(top, tiles[arr[i]].transform) as GameObject;
            newTop.transform.position = tiles[arr[i]].transform.position;
            newTop.name = "Top" + i;
        }

        for (int i = 0; i < tiles.Length; ++i)
        {
            if (tiles[i].transform.childCount.Equals(0))
            {
                gemNum = i;
                GameObject newGem = Instantiate(gem, tiles[i].transform) as GameObject;
                newGem.transform.position = tiles[i].transform.position;
                newGem.GetComponent<SpriteRenderer>().color = GetRandomColor();
                newGem.name = "Gem" + gemNum;
            }
        }
    }

    // 맞는 Gem이 없을 때 돌아가기 위한 시간 기다림 및 처리
    IEnumerator WaitSwapTime()
    {
        StartCoroutine(SwapCoroutine());
        yield return new WaitForSeconds(swapTime + 0.1f);
        destroyGems = new List<GemController>();

        SearchDestroyGem(firGemController._Position, firGemController._Color, firGemController.name);
        destroyGemsCount = destroyGems.Count;

        if (!destroyGemsCount.Equals(0))
        {
            destroyGems.Add(firGemController);
        }

        destroyGemsCount = destroyGems.Count;

        SearchDestroyGem(secGemController._Position, secGemController._Color, secGemController.name);

        if (!destroyGems.Count.Equals(destroyGemsCount))
        {
            destroyGems.Add(secGemController);
        }

        List<GemController> destroyedGems = new List<GemController>();
        foreach (GemController destroyGem in destroyGems)
        {
            if (!destroyGem.CompareTag("TOP"))
            {
                destroyedGems.Add(destroyGem);
                Destroy(destroyGem.gameObject);
                isDestory = true;
            }
        }

        if (!isDestory)
        {
            StartCoroutine(SwapCoroutine());
        }
        else
        {
            List<GameObject> tops = ChangeTop(destroyedGems);
            if (!tops.Count.Equals(0))
            {
                foreach (GameObject top in tops)
                {
                    if (top.GetComponent<SpriteRenderer>().sprite.Equals(top2Img))
                    {
                        top.GetComponent<SpriteRenderer>().sprite = top1Img;
                    }
                    else
                    {
                        destroyedGems.Add(top.GetComponent<GemController>());
                        Destroy(top.gameObject);
                    }
                }
            }

            InitializeGemZen(destroyedGems);
            MoveGem(destroyedGems, destroyedGemsBottom(destroyedGems));
        }
        isDestory = false;
        destroyGemsCount = 0;
    }

    // Gem 재생성
    void InitializeGemZen(List<GemController> destroyedGems)
    {
        List<GemController> zenGems = new List<GemController>();

        foreach (GemController destroyedGem in destroyedGems)
        {
            if (!IsOverlapName(zenGems, destroyedGem.name))
            {
                zenGems.Add(destroyedGem);
            }
        }
        
        foreach (GameObject zenTile in zenTiles)
        {
            int zenCount = 0;
            for (int i = 0; i < zenGems.Count; ++i)
            {
                if (zenGems[i].transform.position.x.Equals(zenTile.transform.position.x))
                {
                    gemNum++;
                    GameObject newGem = Instantiate(gem, zenTile.transform) as GameObject;
                    newGem.transform.position = new Vector2(zenTile.transform.position.x, zenTile.transform.position.y + (0.92f * zenCount));
                    newGem.GetComponent<SpriteRenderer>().color = GetRandomColor();
                    newGem.name = "Gem" + gemNum;
                    zenCount++;
                }
            }
        }
    }

    List<GameObject> ChangeTop(List<GemController> destroyedGems)
    {
        List<GameObject> tops = new List<GameObject>();
        foreach(GemController destroyedGem in destroyedGems)
        {
            Collider2D[] candidateTops = Physics2D.OverlapCircleAll(destroyedGem._Position, 0.93f);
            foreach(Collider2D candidateTop in candidateTops)
            {
                if (candidateTop.tag.Equals("TOP"))
                {
                    if (tops.Count.Equals(0))
                    {
                        tops.Add(candidateTop.gameObject);
                    }
                    else
                    {
                        if(!IsOverlapName(tops, candidateTop.name))
                        {
                            tops.Add(candidateTop.gameObject);
                        }
                    }
                }
            }
        }

        return tops;
    }

    bool IsOverlapName(List<GameObject> checkObjects, string name)
    {
        foreach (GameObject checkObject in checkObjects)
        {
            if (name.Equals(checkObject.name))
            {
                return true;
            }
        }
        return false;
    }

    bool IsOverlapName(List<GemController> checkObjects, string name)
    {
        foreach (GemController checkObject in checkObjects)
        {
            if (name.Equals(checkObject.name))
            {
                return true;
            }
        }
        return false;
    }

    List<GemController> destroyedGemsBottom(List<GemController> destroyedGems)
    {
        destroyedGemsBottoms = new List<GemController>();

        foreach (GemController destroyedGem in destroyedGems)
        {
            int index = EqualsX(destroyedGem, destroyedGemsBottoms);


            if (index.Equals(0))
            {
                destroyedGemsBottoms.Add(destroyedGem);
            }
        }

        return destroyedGemsBottoms;
    }

    int EqualsX(GemController destroyedGem, List<GemController> destroyedGemsBottoms)
    {
        int index = 0;
        foreach (GemController destroyedGemsBottom in destroyedGemsBottoms)
        {
            index++;
            if (destroyedGem._Position.x.Equals(destroyedGemsBottom._Position.x))
            {
                return index;
            }
        }
        return 0;
    }

    // 파괴된 Gem 위치에 Gem을 떨어뜨림
    void MoveGem(List<GemController> destroyedGems, List<GemController> destroyedGemsBottoms)
    {
        foreach (GemController destroyedGemsBottom in destroyedGemsBottoms)
        {
            List<GemController> downGems;
            List<GemController> downDestroyedGems;

            Collider2D[] aboveGems = Physics2D.OverlapBoxAll(new Vector2(destroyedGemsBottom._Position.x, destroyedGemsBottom._Position.y), new Vector2(0.5f, 10f), 0);
            if (!aboveGems.Length.Equals(0))
            {
                downGems = new List<GemController>();
                downDestroyedGems = new List<GemController>();
                foreach (Collider2D aboveGem in aboveGems)
                {
                    int index = 0;

                    if(aboveGem.GetComponent<GemController>()._Position.y < destroyedGemsBottom._Position.y)
                    {
                        aboveGems[index] = null;
                    }
                    else
                    {
                        downGems.Add(aboveGem.GetComponent<GemController>());
                    }
                }

                downGems.Sort(delegate (GemController A, GemController B)
                {
                    if (A._Position.y > B._Position.y) return 1;
                    else if (A._Position.y < B._Position.y) return -1;
                    return 0;
                });

                if (!downGems.Count.Equals(0))
                {
                    foreach (GemController destroyedGem in destroyedGems)
                    {
                        if (destroyedGem._Position.x.Equals(downGems[0]._Position.x))
                        {
                            downDestroyedGems.Add(destroyedGem);
                        }
                    }

                    downDestroyedGems.Sort(delegate (GemController A, GemController B)
                    {
                        if (A._Position.y > B._Position.y) return 1;
                        else if (A._Position.y < B._Position.y) return -1;
                        return 0;
                    });

                    int indexs = 0;
                    List<Transform> downGemSave = new List<Transform>();

                    foreach (GemController downDestroyedGem in downDestroyedGems)
                    {
                        downGemSave.Add(downDestroyedGem.transform.parent);
                    }

                    foreach (GemController downGem in downGems)
                    {
                        downGemSave.Add(downGem.transform.parent);
                    }

                    downGemSave.Sort(delegate (Transform A, Transform B)
                    {
                        if (A.position.y > B.position.y) return 1;
                        else if (A.position.y < B.position.y) return -1;
                        return 0;
                    });

                    foreach (GemController downGem in downGems)
                    {
                        if(indexs.Equals(0) && downGemSave[indexs].name.Equals(downGemSave[indexs + 1].name))
                        {
                            indexs++;
                        }
                        downGem.transform.parent = downGemSave[indexs];
                        indexs++;
                        StartCoroutine(DownCoroutine(downGem));
                    }
                }
            }
        }

    }

    // 주변의 Gem을 탐색하여 삭제하는 함수
    void SearchDestroyGem(Vector2 pos, Color color, string gemControllerName)
    {
        candidates = Physics2D.OverlapCircleAll(pos, 0.93f);
        candidatesEqualsColors = new List<GemController>();
        foreach (Collider2D candidate in candidates)
        {
            if (candidate.GetComponent<SpriteRenderer>().color.Equals(color))
            {
                if (!candidate.name.Equals(gemControllerName))
                {
                    CheckDirection(candidate.transform.position, pos, candidate.gameObject.GetComponent<GemController>());
                }
            }
        }

        foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
        {
            CheckHexDestroyGem(candidatesEqualsColor._Direction, candidatesEqualsColor);
        }

        if (destroyGems.Count.Equals(destroyGemsCount) && candidatesEqualsColors.Count > 2)
        {
            foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
            {
                destroyGems.Add(candidatesEqualsColor);
            }
        }

        if (candidatesEqualsColors.Count.Equals(2) || candidatesEqualsColors.Count.Equals(3))
        {
            GemController firChangeGemController = candidatesEqualsColors[0];
            GemController secChangeGemController = candidatesEqualsColors[1];

            if (candidatesEqualsColors.Count.Equals(3))
            {
                foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
                {
                    if (!InDestoryGemEqual(candidatesEqualsColor))
                    {
                        firChangeGemController = candidatesEqualsColor;
                    }
                }

                if (firChangeGemController._Direction.Equals(GemController.Direction.DOWN))
                {
                    foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
                    {
                        if (candidatesEqualsColor._Direction.Equals(GemController.Direction.LEFTDOWN) ||
                            candidatesEqualsColor._Direction.Equals(GemController.Direction.RIGHTDOWN))
                        {
                            secChangeGemController = candidatesEqualsColor;
                        }
                    }
                }

                if (firChangeGemController._Direction.Equals(GemController.Direction.UP))
                {
                    foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
                    {
                        if (candidatesEqualsColor._Direction.Equals(GemController.Direction.LEFTUP) ||
                            candidatesEqualsColor._Direction.Equals(GemController.Direction.RIGHTUP))
                        {
                            secChangeGemController = candidatesEqualsColor;
                        }
                    }
                }

                if (firChangeGemController._Direction.Equals(GemController.Direction.LEFTDOWN))
                {
                    foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
                    {
                        if (candidatesEqualsColor._Direction.Equals(GemController.Direction.DOWN) ||
                            candidatesEqualsColor._Direction.Equals(GemController.Direction.LEFTUP))
                        {
                            secChangeGemController = candidatesEqualsColor;
                        }
                    }
                }

                if (firChangeGemController._Direction.Equals(GemController.Direction.RIGHTDOWN))
                {
                    foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
                    {
                        if (candidatesEqualsColor._Direction.Equals(GemController.Direction.DOWN) ||
                            candidatesEqualsColor._Direction.Equals(GemController.Direction.RIGHTUP))
                        {
                            secChangeGemController = candidatesEqualsColor;
                        }
                    }
                }

                if (firChangeGemController._Direction.Equals(GemController.Direction.RIGHTUP))
                {
                    foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
                    {
                        if (candidatesEqualsColor._Direction.Equals(GemController.Direction.UP) ||
                            candidatesEqualsColor._Direction.Equals(GemController.Direction.RIGHTDOWN))
                        {
                            secChangeGemController = candidatesEqualsColor;
                        }
                    }
                }

                if (firChangeGemController._Direction.Equals(GemController.Direction.LEFTUP))
                {
                    foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
                    {
                        if (candidatesEqualsColor._Direction.Equals(GemController.Direction.UP) ||
                            candidatesEqualsColor._Direction.Equals(GemController.Direction.LEFTDOWN))
                        {
                            secChangeGemController = candidatesEqualsColor;
                        }
                    }
                }
            }

            if (firChangeGemController._Direction < secChangeGemController._Direction)
            {
                GemController changeGemController = firChangeGemController;
                firChangeGemController = secChangeGemController;
                secChangeGemController = changeGemController;
            }

            if (firChangeGemController._Direction.Equals(GemController.Direction.DOWN) &&
                secChangeGemController._Direction.Equals(GemController.Direction.LEFTDOWN))
            {
                Collider2D candidateNext = Physics2D.OverlapBox(new Vector2(firChangeGemController._Position.x - 0.6f,
                    firChangeGemController._Position.y - 0.3f), new Vector2(0.5f, 0.5f), 0);
                if (candidateNext && candidateNext.GetComponent<SpriteRenderer>().color.Equals(color))
                {
                    destroyGems.Add(candidateNext.gameObject.GetComponent<GemController>());
                    destroyGems.Add(firChangeGemController);
                    destroyGems.Add(secChangeGemController);
                }
            }

            if (firChangeGemController._Direction.Equals(GemController.Direction.DOWN) &&
                secChangeGemController._Direction.Equals(GemController.Direction.RIGHTDOWN))
            {
                Collider2D candidateNext = Physics2D.OverlapBox(new Vector2(firChangeGemController._Position.x + 0.6f,
                    firChangeGemController._Position.y - 0.3f), new Vector2(0.5f, 0.5f), 0);
                if (candidateNext && candidateNext.GetComponent<SpriteRenderer>().color.Equals(color))
                {
                    destroyGems.Add(candidateNext.gameObject.GetComponent<GemController>());
                    destroyGems.Add(firChangeGemController);
                    destroyGems.Add(secChangeGemController);
                }
            }

            if (firChangeGemController._Direction.Equals(GemController.Direction.LEFTDOWN) &&
                secChangeGemController._Direction.Equals(GemController.Direction.LEFTUP))
            {
                Collider2D candidateNext = Physics2D.OverlapBox(new Vector2(firChangeGemController._Position.x - 0.6f,
                    firChangeGemController._Position.y + 0.3f), new Vector2(0.5f, 0.5f), 0);
                if (candidateNext && candidateNext.GetComponent<SpriteRenderer>().color.Equals(color))
                {
                    destroyGems.Add(candidateNext.gameObject.GetComponent<GemController>());
                    destroyGems.Add(firChangeGemController);
                    destroyGems.Add(secChangeGemController);
                }
            }

            if (firChangeGemController._Direction.Equals(GemController.Direction.LEFTUP) &&
                secChangeGemController._Direction.Equals(GemController.Direction.UP))
            {
                Collider2D candidateNext = Physics2D.OverlapBox(new Vector2(firChangeGemController._Position.x,
                    firChangeGemController._Position.y + 0.7f), new Vector2(0.5f, 0.5f), 0);
                if (candidateNext && candidateNext.GetComponent<SpriteRenderer>().color.Equals(color))
                {
                    destroyGems.Add(candidateNext.gameObject.GetComponent<GemController>());
                    destroyGems.Add(firChangeGemController);
                    destroyGems.Add(secChangeGemController);
                }
            }

            if (firChangeGemController._Direction.Equals(GemController.Direction.RIGHTUP) &&
                secChangeGemController._Direction.Equals(GemController.Direction.UP))
            {
                Collider2D candidateNext = Physics2D.OverlapBox(new Vector2(firChangeGemController._Position.x,
                    firChangeGemController._Position.y + 0.7f), new Vector2(0.5f, 0.5f), 0);
                if (candidateNext && candidateNext.GetComponent<SpriteRenderer>().color.Equals(color))
                {
                    destroyGems.Add(candidateNext.gameObject.GetComponent<GemController>());
                    destroyGems.Add(firChangeGemController);
                    destroyGems.Add(secChangeGemController);
                }
            }

            if (firChangeGemController._Direction.Equals(GemController.Direction.RIGHTDOWN) &&
                secChangeGemController._Direction.Equals(GemController.Direction.RIGHTUP))
            {
                Collider2D candidateNext = Physics2D.OverlapBox(new Vector2(firChangeGemController._Position.x + 0.6f,
                    firChangeGemController._Position.y + 0.3f), new Vector2(0.5f, 0.5f), 0);
                if (candidateNext && candidateNext.GetComponent<SpriteRenderer>().color.Equals(color))
                {
                    destroyGems.Add(candidateNext.gameObject.GetComponent<GemController>());
                    destroyGems.Add(firChangeGemController);
                    destroyGems.Add(secChangeGemController);
                }
            }
        }

        foreach (GemController candidatesEqualsColor in candidatesEqualsColors)
        {
            CheckHexNextDestroyGem(candidatesEqualsColor._Direction, candidatesEqualsColor.transform.position, 
                candidatesEqualsColor, candidatesEqualsColor.GetComponent<SpriteRenderer>().color);
        }

    }

    bool InDestoryGemEqual(GemController candidatesEqualsColor)
    {
        foreach (GemController destroyGem in destroyGems)
        {
            if (candidatesEqualsColor.name == destroyGem.name)
            {
                return true;
            }
        }
        return false;
    }

    void CheckDirection(Vector2 aroundGemPos, Vector2 centerGemPos, GemController candidate)
    {
        // up
        if (centerGemPos.x == aroundGemPos.x && centerGemPos.y + 0.7f <= aroundGemPos.y)
        {
            candidate._Direction = GemController.Direction.UP;
            candidatesEqualsColors.Add(candidate);
        }

        //down
        if (centerGemPos.x == aroundGemPos.x && centerGemPos.y - 0.7f >= aroundGemPos.y)
        {
            candidate._Direction = GemController.Direction.DOWN;
            candidatesEqualsColors.Add(candidate);
        }

        // left down
        if (centerGemPos.x - 0.6f >= aroundGemPos.x && centerGemPos.y - 0.3f >= aroundGemPos.y)
        {
            candidate._Direction = GemController.Direction.LEFTDOWN;
            candidatesEqualsColors.Add(candidate);
        }

        // left up
        if (centerGemPos.x - 0.6f >= aroundGemPos.x && centerGemPos.y + 0.3f <= aroundGemPos.y)
        {
            candidate._Direction = GemController.Direction.LEFTUP;
            candidatesEqualsColors.Add(candidate);
        }

        // right down
        if (centerGemPos.x + 0.5f <= aroundGemPos.x && centerGemPos.y - 0.3f >= aroundGemPos.y)
        {
            candidate._Direction = GemController.Direction.RIGHTDOWN;
            candidatesEqualsColors.Add(candidate);
        }

        // right up
        if (centerGemPos.x + 0.5f <= aroundGemPos.x && centerGemPos.y + 0.3f <= aroundGemPos.y)
        {
            candidate._Direction = GemController.Direction.RIGHTUP;
            candidatesEqualsColors.Add(candidate);
        }
    }

    void CheckHexDestroyGem(GemController.Direction direction, GemController candidatesEqualsColor)
    {
        if (direction.Equals(GemController.Direction.DOWN))
        {
            foreach (GemController candidatesEqualsColorNext in candidatesEqualsColors)
            {
                if (candidatesEqualsColorNext._Direction.Equals(GemController.Direction.UP))
                {
                    destroyGems.Add(candidatesEqualsColorNext);
                    destroyGems.Add(candidatesEqualsColor);
                }
            }
        }
        else if (direction.Equals(GemController.Direction.LEFTDOWN))
        {
            foreach (GemController candidatesEqualsColorNext in candidatesEqualsColors)
            {
                if (candidatesEqualsColorNext._Direction.Equals(GemController.Direction.RIGHTUP))
                {
                    destroyGems.Add(candidatesEqualsColorNext);
                    destroyGems.Add(candidatesEqualsColor);
                }
            }
        }
        else if (direction.Equals(GemController.Direction.LEFTUP))
        {
            foreach (GemController candidatesEqualsColorNext in candidatesEqualsColors)
            {
                if (candidatesEqualsColorNext._Direction.Equals(GemController.Direction.RIGHTDOWN))
                {
                    destroyGems.Add(candidatesEqualsColorNext);
                    destroyGems.Add(candidatesEqualsColor);
                }
            }
        }
    }

    void CheckHexNextDestroyGem(GemController.Direction direction, Vector2 aroundGemPos, GemController candidatesEqualsColor, Color color)
    {
        Collider2D candidateNext;
        if (direction.Equals(GemController.Direction.UP))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x, aroundGemPos.y + 0.7f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, candidatesEqualsColor, direction, color);
        }
        else if (direction.Equals(GemController.Direction.DOWN))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x, aroundGemPos.y - 0.7f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, candidatesEqualsColor, direction, color);
        }
        else if (direction.Equals(GemController.Direction.LEFTDOWN))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x - 0.6f, aroundGemPos.y - 0.3f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, candidatesEqualsColor, direction, color);
        }
        else if (direction.Equals(GemController.Direction.LEFTUP))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x - 0.6f, aroundGemPos.y + 0.3f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, candidatesEqualsColor, direction, color);
        }
        else if (direction.Equals(GemController.Direction.RIGHTDOWN))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x + 0.6f, aroundGemPos.y - 0.3f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, candidatesEqualsColor, direction, color);
        }
        else if (direction.Equals(GemController.Direction.RIGHTUP))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x + 0.6f, aroundGemPos.y + 0.3f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, candidatesEqualsColor, direction, color);
        }
    }

    void CheckHexNextDestroyGem(GemController.Direction direction, Vector2 aroundGemPos, Color color)
    {
        Collider2D candidateNext;
        if (direction.Equals(GemController.Direction.UP))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x, aroundGemPos.y + 0.7f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, direction, color);
        }
        else if (direction.Equals(GemController.Direction.DOWN))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x, aroundGemPos.y - 0.7f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, direction, color);
        }
        else if (direction.Equals(GemController.Direction.LEFTDOWN))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x - 0.6f, aroundGemPos.y - 0.3f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, direction, color);
        }
        else if (direction.Equals(GemController.Direction.LEFTUP))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x - 0.6f, aroundGemPos.y + 0.3f), new Vector2(0.5f, 0.5f), 0);
            CandidateNextAddDestory(candidateNext, direction, color);
        }
        else if (direction.Equals(GemController.Direction.RIGHTDOWN))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x + 0.6f, aroundGemPos.y - 0.3f), new Vector2(0, 0), 0);
            CandidateNextAddDestory(candidateNext, direction, color);
        }
        else if (direction.Equals(GemController.Direction.RIGHTUP))
        {
            candidateNext = Physics2D.OverlapBox(new Vector2(aroundGemPos.x + 0.6f, aroundGemPos.y + 0.3f), new Vector2(0, 0), 0);
            CandidateNextAddDestory(candidateNext, direction, color);
        }
    }

    // 주변 6범위 & 그이상 범위 추가
    void CandidateNextAddDestory(Collider2D candidateNext, GemController candidatesEqualsColor, GemController.Direction direction, Color color)
    {
        if (candidateNext && candidateNext.GetComponent<SpriteRenderer>().color.Equals(color))
        {
            destroyGems.Add(candidateNext.gameObject.GetComponent<GemController>());
            if (!DestroyOverlapCheck(candidatesEqualsColor.name))
            {
                destroyGems.Add(candidatesEqualsColor);
            }
            CheckHexNextDestroyGem(direction, candidateNext.transform.position, color);
        }
    }

    void CandidateNextAddDestory(Collider2D candidateNext, GemController.Direction direction, Color color)
    {
        if (candidateNext && candidateNext.GetComponent<SpriteRenderer>().color.Equals(color))
        {
            destroyGems.Add(candidateNext.gameObject.GetComponent<GemController>());
            CheckHexNextDestroyGem(direction, candidateNext.transform.position, color);
        }
    }

    // 중복 검사
    bool DestroyOverlapCheck(string candidateName)
    {
        foreach (GemController destroyGem in destroyGems)
        {
            if (destroyGem.name.Equals(candidateName))
            {
                return true;
            }
        }
        return false;
    }

    // 서로 위치를 바꾸는 함수
    IEnumerator SwapCoroutine()
    {
        ChangeParent(firGemController._Parents, secGemController._Parents);

        float startTime = Time.time;
        Vector2 firPos = firGemController._LocalPosition;
        Vector2 secPos = secGemController._LocalPosition;

        while (Time.time - startTime <= swapTime)
        {
            firGemController._LocalPosition = Vector2.Lerp(firPos, Vector2.zero, (Time.time - startTime) * swapSpeed / swapTime);
            secGemController._LocalPosition = Vector2.Lerp(secPos, Vector2.zero, (Time.time - startTime) * swapSpeed / swapTime);
            yield return null;
        }
    }

    // 아래로 떨어지는 함수
    IEnumerator DownCoroutine(GemController downGem)
    {
        float startTime = Time.time;
        Vector2 pos = downGem._LocalPosition;

        while (Time.time - startTime <= swapTime)
        {
            downGem._LocalPosition = Vector2.Lerp(pos, Vector2.zero, (Time.time - startTime) * swapSpeed / swapTime);
            yield return null;
        }
    }

    // 아래로 얼마나 떨어질지 정해서 떨어지는 함수
    IEnumerator DownCoroutine(GameObject downGem, float down, float left, float right)
    {
        float startTime = Time.time;
        Vector2 pos = downGem.transform.position;
        Vector2 nextPos = Vector2.zero;
        if (left.Equals(0) && right.Equals(0))
        {
            nextPos = new Vector2(pos.x, pos.y - 0.765f);
        }
        else if (!left.Equals(0) && right.Equals(0))
        {
            nextPos = new Vector2(pos.x, pos.y - 0.765f);
        }
        else if (left.Equals(0) && !right.Equals(0))
        {
            nextPos = new Vector2(pos.x, pos.y - 0.765f);
        }

        while (Time.time - startTime <= swapTime)
        {
            downGem.transform.position = Vector2.Lerp(pos, nextPos, 
                (Time.time - startTime) * swapSpeed / swapTime);
            yield return null;
        }
        isDownDone = true;
    }

    // 다 떨어진후 실행
    IEnumerator EndDown(List<GameObject> newGems)
    {
        yield return new WaitForSeconds(1f);
        bool isDone = false;
        while (!isDone)
        {
            foreach(GameObject newGem in newGems)
            {
                Collider2D candidateNext;
                candidateNext = Physics2D.OverlapBox(new Vector2(newGem.transform.position.x, newGem.transform.position.y + 0.7f), new Vector2(0.5f, 0.5f), 0);
                if (candidateNext)
                {
                    candidateNext = Physics2D.OverlapBox(new Vector2(newGem.transform.position.x - 0.6f, newGem.transform.position.y - 0.3f), new Vector2(0.5f, 0.5f), 0);
                    if (candidateNext)
                    {
                        candidateNext = Physics2D.OverlapBox(new Vector2(newGem.transform.position.x + 0.6f, newGem.transform.position.y - 0.3f), new Vector2(0.5f, 0.5f), 0);
                    }
                }
                else
                {

                }
            }
        }
        
    }

    // 서로의 부모를 바꿔는 함수
    void ChangeParent(Transform firGemParnet, Transform secGemParnet)
    {
        Transform firGemParent = firGemController._Parents;
        firGemController._Parents = secGemController._Parents;
        secGemController._Parents = firGemParent;
    }
}
