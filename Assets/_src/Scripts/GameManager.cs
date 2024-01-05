using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Screen = UnityEngine.Device.Screen;

public class GameManager : MonoBehaviour
{
    public AudioSource audioSource;
    
    [Space(20)]
    [Range(4, 10)] 
    public int numberOfColors;
    public Color[] colors;
    public float circleRotationDuration= 0.1f;

    public Transform circleContainer;
    public Slice slicePrefab;

    [Space(20)] 
    public Point pointPrefab;
    public Transform pointsContainer;
    [FormerlySerializedAs("spawnRange")] public float spawnRadius;
    public Vector2 speedRange;
    public Vector2 timeToSpawnPoints;
    public int timeMaxDifficulty = 60;


    [Space(20)] 
    public GameObject playButton;
    public GameObject highScoreMessage;
    public TMP_Text score;

    [Space(20)] 
    public GameObject tutorialScreen;

    [Space(20)] 
    public GameObject gameOverScreen;
    public TMP_Text gameOverScore;
    public Image gameOverTimer;
    public float gameOverScreenTime;
    
    private AdManager _adManager;
    
    private float _screenMiddle;
    private float _angleOffset;

    private List<Point> _points = new();

    private bool _isPlaying;
    private float _spawnTimer;

    private int _score;
    private float _gameTimer;

    private float _circleRotation = 0;

    private Sequence _gameOverSequence;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        _adManager = FindObjectOfType<AdManager>();
        _adManager.SetRewardAdCallback(RewardAdCallback);
        
        _screenMiddle = Screen.width / 2;
        
        InitializeCircle();

        SetStartUI();

        // will show the tutorial the first time the game is opened
        if (PlayerPrefs.GetInt("showTutorial", 1) == 1)
        {
            ShowTutorial();
            PlayerPrefs.SetInt("showTutorial",0);
        }
    }

    private void Update()
    {
        if (!_isPlaying) return;
        
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.mousePosition.x - _screenMiddle > 0)
            {
                MoveCirle(-1);
            }
            else
            {
                MoveCirle(1);
            }
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            MoveCirle(-1);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            MoveCirle(1);
        }

        _gameTimer += Time.deltaTime;
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0)
        {
            SpawnPoint();
        }
    }

    // Sets the UI to the default state
    private void SetStartUI()
    {
        _isPlaying = false;
        playButton.SetActive(true);
        highScoreMessage.SetActive(true);
        score.text = PlayerPrefs.GetInt("highScore", 0).ToString();
    }
    
    // Create the slices of the circle
    private void InitializeCircle()
    {

        float size = 1f / numberOfColors;
        _angleOffset = 360f / numberOfColors;
        
        for (int i = 0; i < numberOfColors; i++)
        {
            Slice s = Instantiate(slicePrefab, circleContainer);
            s.Init(size, _angleOffset,_angleOffset * i, colors[i], OnColorCatch);
        }

        circleContainer.localEulerAngles = new Vector3(0, 0, _angleOffset / 2);
    }

    private void MoveCirle(int direction)
    {
        float targetDegree = (_circleRotation + _angleOffset * direction) % 360;
        if (targetDegree < 0)
        {
            targetDegree += 360;
        }

        circleContainer.DOLocalRotate(new Vector3(0, 0, targetDegree), circleRotationDuration).onComplete = UpdateCircleRotation;
    }
    private void UpdateCircleRotation()
    {
        _circleRotation = circleContainer.localEulerAngles.z;
    }

    private void SpawnPoint()
    {
        Point p = GetPoint();
        Vector3 spawnPosition = GetRandomPointStartPosition();
        
        p.Init(GetRandomColor(),spawnPosition,GetPointSpeed(),-spawnPosition.normalized);

        _spawnTimer = GetNewSpawnTime();
    }
    //Recovers a point from the pool is instantiate a new one
    private Point GetPoint()
    {
        if (_points.Count > 0)
        {
            Point p = _points[0];
            _points.RemoveAt(0);
            return p;
        }

        Point newPoint = Instantiate(pointPrefab, pointsContainer);
        newPoint.Create(AddToPool);
        return newPoint;
    }
    // Once the point has been destroyed it will call this function it will be added to the pool
    private void AddToPool(Point p)
    {
        _points.Add(p);
    }

    
    private Color GetRandomColor()
    {
        return colors[Random.Range(0, numberOfColors)];
    }
    private Vector3 GetRandomPointStartPosition()
    {
        float angle = _angleOffset * Random.Range(0, numberOfColors);
        
        float pointX = spawnRadius * Mathf.Sin(Mathf.Deg2Rad*angle);
        float pointY = spawnRadius * Mathf.Cos(Mathf.Deg2Rad*angle);

        return new Vector3(pointX, pointY, 0);

    }
    
    // Gets speed and spawn time based of difficulty
    private float GetPointSpeed()
    {
        float range = speedRange.y - speedRange.x;
        return speedRange.x + range*GetDifficulty();
    }
    private float GetNewSpawnTime()
    {
        float range = timeToSpawnPoints.y - timeToSpawnPoints.x;
        return timeToSpawnPoints.y - range*GetDifficulty();
    }
    //Difficulty calculated based of the time since the start of the game, once it reaced the value of the variable timeMaxDifficulty it will be at difficulty 1 which is the max 
    private float GetDifficulty()
    {
        float currentTime = Mathf.Clamp(_gameTimer,0, timeMaxDifficulty);
        return currentTime / timeMaxDifficulty;
    }
    
    //Callback when a point collided with the circle. true if the color matched else false
    private void OnColorCatch(bool rightColor)
    {
        if (rightColor)
        {
            _score++;
            score.text = _score.ToString();
        }
        else
        {
            _points.Clear();

            for (int i = pointsContainer.childCount -1; i >= 0 ; i--)
            {
                Destroy(pointsContainer.GetChild(i).gameObject);
            }
            
            if (_score > PlayerPrefs.GetInt("highScore", 0))
            {
                PlayerPrefs.SetInt("highScore",_score);
            }
            
            
            _isPlaying = false;
            gameOverScreen.SetActive(true);
            gameOverScore.text = $"YOUR SCORE IS: {_score}";
            gameOverTimer.fillAmount = 1;
            _gameOverSequence = DOTween.Sequence();
            _gameOverSequence.Append(gameOverTimer.DOFillAmount(0, gameOverScreenTime).SetEase(Ease.Linear))
                .onComplete = CloseGameOverScreen;
        }
        
        audioSource.Play();
    }

    private void CloseGameOverScreen()
    {
        gameOverScreen.SetActive(false);
        SetStartUI();
    }
    
    public void StartGame()
    {
        playButton.SetActive(false);
        highScoreMessage.SetActive(false);

        _score = 0;
        
        SetGame();
    }

    public void TryToRevive()
    {
        _gameOverSequence.Kill();
        _adManager.ShowRewardAd();
    }
    // called when the player watched the reward ads and if the ad has been completed then the user is revived
    private void RewardAdCallback(bool successful)
    {
        if(successful) Revive();
        else CloseGameOverScreen();
    }
    private void Revive()
    {
        gameOverScreen.SetActive(false);
        SetGame();
    }

    private void SetGame()
    {
        _circleRotation = _angleOffset / 2;
        circleContainer.localEulerAngles = new Vector3(0, 0, _circleRotation);
        
        score.text = _score.ToString();

        _gameTimer = 0;
        _isPlaying = true;
        
        _spawnTimer = GetNewSpawnTime();
    }


    public void ShowTutorial()
    {
        tutorialScreen.SetActive(true);
    }
    public void CloseTutorial()
    {
        tutorialScreen.SetActive(false);
    }
}
