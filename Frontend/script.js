// ---------------------- CANVAS SETUP ----------------------
const canvas = document.getElementById("gameCanvas");
const ctx = canvas.getContext("2d");

// ---------------------- UI ELEMENTS ----------------------
const startBtn = document.getElementById("startBtn");
const restartBtn = document.getElementById("restartBtn");
const scoreText = document.getElementById("score");
const highScoreText = document.getElementById("highScore");
const timeText = document.getElementById("timeLeft");
const savePlayerBtn = document.getElementById("savePlayerBtn");
const playerNameInput = document.getElementById("playerName");
const playerPasswordInput = document.getElementById("playerPassword");

// ---------------------- IMAGES ----------------------
const playerImg = new Image(); playerImg.src = "assets/player.png";
const catImg = new Image(); catImg.src = "assets/cat.png";
const badCatImg = new Image(); badCatImg.src = "assets/badcat.png";
const fatCatImg = new Image(); fatCatImg.src = "assets/fatcat.png";

// ---------------------- PLAYER ----------------------
let player = { x: 400, y: 250, width: 50, height: 50, speed: 4 };

// ---------------------- GAME STATE ----------------------
let cats = [];
let score = 0;
let highScore = 0;
let timeLeft = 60;
let gameOver = false;
let gameRunning = false;

let goodCats = 0;
let badCats = 0;
let fatCats = 0;

let floatTexts = [];
let badCatChance = 0.25;
let speedNotice = "";
let speedNoticeTime = 0;

let keys = {};
document.addEventListener("keydown", e => keys[e.key] = true);
document.addEventListener("keyup", e => keys[e.key] = false);

// ---------------------- API ----------------------
const API_URL = "http://localhost:5202/api";
let token = "";
let playerId = null;

// ---------------------- AUTH ----------------------
async function registerPlayer(name, password) {
  return fetch(`${API_URL}/auth/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, password })
  });
}

async function loginPlayer(name, password) {
  const res = await fetch(`${API_URL}/auth/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, password })
  });
  if (!res.ok) return {};
  const data = await res.json();
  token = data.token;
  return data;
}

// ---------------------- PLAYERS ----------------------
async function getPlayers() {
  const res = await fetch(`${API_URL}/players`, {
    headers: { Authorization: `Bearer ${token}` }
  });
  return await res.json();
}

async function createPlayer(name) {
  const res = await fetch(`${API_URL}/players/create`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify({ name })
  });
  return await res.json();
}

async function updatePlayerScore() {
  if (!playerId) return;
  await fetch(`${API_URL}/players/${playerId}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`
    },
    body: JSON.stringify({
      goodCatsCollected: goodCats,
      badCatsCollected: badCats,
      chonkyCatsCollected: fatCats,
      bestScore: score
    })
  });
  loadLeaderboard();
}

// ---------------------- LEADERBOARD ----------------------
async function loadLeaderboard() {
  if (!token) return;
  const players = await getPlayers();
  const tbody = document.querySelector("#leaderboard tbody");
  tbody.innerHTML = players.map(p => `
    <tr>
      <td>${p.name}</td>
      <td>${p.bestScore}</td>
      <td>${p.goodCatsCollected}</td>
      <td>${p.badCatsCollected}</td>
      <td>${p.chonkyCatsCollected}</td>
    </tr>
  `).join("");
}

// ---------------------- LOGIN FLOW ----------------------
savePlayerBtn.addEventListener("click", async () => {
  const name = playerNameInput.value.trim();
  const password = playerPasswordInput.value.trim();

  if (!name || !password) return alert("Enter name and password");

  let login = await loginPlayer(name, password);
  if (!login.token) {
    await registerPlayer(name, password);
    login = await loginPlayer(name, password);
  }

  const playerData = await createPlayer(name);
  playerId = playerData.id;

  loadLeaderboard();
  document.getElementById("playerNameContainer").style.display = "none";
});

// ---------------------- GAME LOGIC ----------------------
// Spawn cats
function spawnCat() {
  if (gameOver) return;

  let isFat = Math.random() < 0.10;
  let isBad = !isFat && Math.random() < badCatChance;

  let width = isFat ? 60 : 48;
  let height = isFat ? 60 : 48;

  let dx = (Math.random() * 2 + 1) * (Math.random() < 0.5 ? 1 : -1);
  let dy = (Math.random() * 2 + 1) * (Math.random() < 0.5 ? 1 : -1);

  cats.push({
    x: Math.random() * (canvas.width - width),
    y: Math.random() * (canvas.height - height),
    width,
    height,
    bad: isBad,
    fat: isFat,
    dx,
    dy,
    spawnTime: Date.now()
  });
}

// Player movement
function updatePlayer() {
  if (keys["ArrowUp"] || keys["w"]) player.y -= player.speed;
  if (keys["ArrowDown"] || keys["s"]) player.y += player.speed;
  if (keys["ArrowLeft"] || keys["a"]) player.x -= player.speed;
  if (keys["ArrowRight"] || keys["d"]) player.x += player.speed;

  player.x = Math.max(0, Math.min(canvas.width - player.width, player.x));
  player.y = Math.max(0, Math.min(canvas.height - player.height, player.y));
}

// Update cats
function updateCats() {
  cats.forEach(c => {
    c.x += c.dx;
    c.y += c.dy;

    if (c.x < 0 || c.x + c.width > canvas.width) c.dx *= -1;
    if (c.y < 0 || c.y + c.height > canvas.height) c.dy *= -1;
  });
}

// Floating texts
function addFloatText(text, x, y, color) {
  floatTexts.push({ text, x, y, alpha: 1, dy: -1, color });
}

function updateFloatTexts() {
  floatTexts.forEach(t => { t.y += t.dy; t.alpha -= 0.02; });
  floatTexts = floatTexts.filter(t => t.alpha > 0);
}

// Collision detection
function checkCollisions() {
  for (let i = cats.length - 1; i >= 0; i--) {
    const c = cats[i];
    const hitboxShrink = 10;

    const catLeft = c.x + hitboxShrink;
    const catRight = c.x + c.width - hitboxShrink;
    const catTop = c.y + hitboxShrink;
    const catBottom = c.y + c.height - hitboxShrink;

    const playerLeft = player.x;
    const playerRight = player.x + player.width;
    const playerTop = player.y;
    const playerBottom = player.y + player.height;

    if (playerRight > catLeft && playerLeft < catRight &&
        playerBottom > catTop && playerTop < catBottom) {

      if (c.fat) { score += 2; fatCats++; addFloatText("+2", c.x, c.y, "orange"); }
      else if (c.bad) { score -= 1; badCats++; addFloatText("-1", c.x, c.y, "red"); }
      else { score += 1; goodCats++; addFloatText("+1", c.x, c.y, "green"); }

      cats.splice(i, 1);
      scoreText.textContent = score;
    }
  }
}

// Draw everything
function draw() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);

  const now = Date.now();
  cats = cats.filter(c => now - c.spawnTime < 5000);

  cats.forEach(c => {
    const img = c.fat ? fatCatImg : (c.bad ? badCatImg : catImg);
    ctx.drawImage(img, c.x, c.y, c.width, c.height);
  });

  ctx.drawImage(playerImg, player.x, player.y, player.width, player.height);

  floatTexts.forEach(t => {
    ctx.globalAlpha = t.alpha;
    ctx.fillStyle = t.color;
    ctx.font = "24px Arial";
    ctx.fillText(t.text, t.x, t.y);
    ctx.globalAlpha = 1;
  });

  ctx.fillStyle = "black";
  ctx.font = "20px Arial";
  ctx.fillText(`Score: ${score}`, 20, 30);
  ctx.fillText(`Time Left: ${timeLeft}`, 20, 60);

  if (Date.now() - speedNoticeTime < 1200) {
    ctx.fillStyle = "yellow";
    ctx.font = "30px Arial";
    ctx.fillText(speedNotice, 330, 50);
  }

  if (gameOver) {
    ctx.fillStyle = "rgba(0,0,0,0.5)";
    ctx.fillRect(0, 0, canvas.width, canvas.height);

    ctx.fillStyle = "white";
    ctx.font = "48px Arial";
    ctx.fillText("GAME OVER", 260, 240);

    ctx.font = "32px Arial";
    ctx.fillText(`Final Score: ${score}`, 300, 290);

    if (score > highScore) {
      highScore = score;
      highScoreText.textContent = highScore;
    }
  }

  timeText.textContent = timeLeft;
}

// Game loop
function gameLoop() {
  if (!gameOver) {
    updatePlayer();
    updateCats();
    checkCollisions();
    updateFloatTexts();
  }
  draw();
  requestAnimationFrame(gameLoop);
}

// Timer & spawn intervals
let timerInterval, spawnInterval, extraSpawn1, extraSpawn2;

// Restart game
function restartGame() {
  clearInterval(timerInterval);
  clearInterval(spawnInterval);
  clearInterval(extraSpawn1);
  clearInterval(extraSpawn2);

  player.x = 400;
  player.y = 250;
  player.speed = 4;
  score = 0;
  goodCats = 0;
  badCats = 0;
  fatCats = 0;
  timeLeft = 60;
  cats = [];
  floatTexts = [];
  badCatChance = 0.25;
  speedNotice = "";
  speedNoticeTime = 0;
  gameOver = false;

  scoreText.textContent = score;
  timeText.textContent = timeLeft;

  // Timer
  timerInterval = setInterval(() => {
    if (!gameOver && timeLeft > 0) {
      timeLeft--;

      if (timeLeft % 10 === 0) {
        player.speed += 0.5;
        speedNotice = "Speed Up!";
        speedNoticeTime = Date.now();

        badCatChance += 0.10;
        if (badCatChance > 0.7) badCatChance = 0.7;
      }
    }

    // GAME OVER — save score only here
    if (timeLeft <= 0 && !gameOver) {
      gameOver = true;
      saveScore(); // ✅ send final score
    }
  }, 1000);

  spawnInterval = setInterval(spawnCat, 1000);
  extraSpawn1 = setInterval(spawnCat, 1100);
  extraSpawn2 = setInterval(spawnCat, 1200);

  canvas.focus();
}

// Save score to backend
function saveScore() {
  if (playerId) {
    updatePlayerScore();
  }
}

// Button events
startBtn.addEventListener("click", () => {
  if (!gameRunning) {
    gameRunning = true;
    gameLoop();
  }
  restartGame();
});
restartBtn.addEventListener("click", restartGame);
