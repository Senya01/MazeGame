using System;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Rigidbody2D rigidbody;
    [SerializeField] private int difficulty;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject blackoutGameObject;

    [SerializeField] private bool pathHint;
    [SerializeField] private bool blackout;

    [SerializeField] private MazeGenerator mazeGenerator;

    [HideInInspector] private Vector2 moveDirection;
    [HideInInspector] private int score;
    [HideInInspector] private int totalTime;

    [HideInInspector] private float timerLeft;

    private void Update()
    {
        ProcessMovementInputs();
        CheckForWin();
        Timer();
    }

    private void Start()
    {
        UpdateTimeText();
        
        blackoutGameObject.SetActive(blackout);
    }

    private void Timer()
    {
        if (timerLeft > 0)
        {
            timerLeft -= Time.deltaTime;
        }
        else
        {
            timerLeft = 1;
            totalTime++;
            UpdateTimeText();
        }
    }

    private void UpdateTimeText()
    {
        TimeSpan t = TimeSpan.FromSeconds(totalTime);

        timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}",
            t.Hours,
            t.Minutes,
            t.Seconds);
    }

    private void AddScore()
    {
        score++;
        scoreText.text = $"Score: {score}";
    }

    private void CheckForWin()
    {
        if (transform.position.x > mazeGenerator.mazeRows / 2 || transform.position.x < -(mazeGenerator.mazeRows / 2) ||
            transform.position.y > mazeGenerator.mazeColumns / 2 ||
            transform.position.y < -(mazeGenerator.mazeColumns / 2))
        {
            transform.position = Vector3.zero;
            mazeGenerator.mazeRows += difficulty;
            mazeGenerator.mazeColumns += difficulty;
            mazeGenerator.GenerateMaze();
            AddScore();
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ProcessMovementInputs()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY);
    }

    private void Move()
    {
        rigidbody.velocity = new Vector2(moveDirection.x * moveSpeed, moveDirection.y * moveSpeed);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (pathHint && col.tag == "Cell")
        {
            col.GetComponent<SpriteRenderer>().enabled = true;
        }
    }
}