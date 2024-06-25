<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Multiplayer Word Game Readme</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            line-height: 1.6;
            margin: 20px;
        }
        h1, h2, h3 {
            color: #333;
        }
        a {
            color: #0066cc;
            text-decoration: none;
        }
        a:hover {
            text-decoration: underline;
        }
        ul {
            margin: 10px 0;
            padding: 0 0 0 20px;
        }
    </style>
</head>
<body>

<h1>Multiplayer Word Game</h1>

<h2>Technologies</h2>
<ul>
    <li>ABP Framework</li>
    <li>ASP.NET Core</li>
    <li>EFCore</li>
    <li>SignalR</li>
</ul>

<h2>Overview</h2>
<p>This project is a multiplayer-based, competitive word game. After registering, users can join one of the available lobbies and send match requests to other players in the lobby.</p>
<p>This repository is for the backend side of the project. There is also a Flutter repository available: <a href="https://github.com/FatihGeylan/multiplayer_word_game" target="_blank">Flutter Repository</a></p>

<h2>Lobbies</h2>
<p>There are a total of 8 lobbies with 2 different game modes. These lobbies vary according to word length and game mode.</p>

<h3>Available Word Lengths for Lobbies</h3>
<ul>
    <li>4 letters</li>
    <li>5 letters</li>
    <li>6 letters</li>
    <li>7 letters</li>
</ul>

<h3>Available Game Modes</h3>
<ul>
    <li>Classic</li>
    <li>Random Letter Generated</li>
</ul>

<h2>Game Modes</h2>

<h3>Classic Game Mode</h3>
<p>Players can select their desired words, which should match the word length defined for the current lobby.</p>

<h3>Random Letter Generated Game Mode</h3>
<p>A random letter will be placed in a random order. Players need to find a suitable word containing the selected letter in the correct order and matching the word length defined for the current lobby.</p>

<h2>Gameplay</h2>
<ul>
    <li>After words are selected, players try to guess their opponent's selected word. Each player has 5 chances.</li>
    <li>During the game, players can see their opponent's guesses.</li>
    <li>Whenever a player inputs a guess, their guess score is calculated as follows:
        <ul>
            <li>Containing the letter in the correct order: 10 points (Displayed as green)</li>
            <li>Containing the letter in the incorrect order: 5 points (Displayed as yellow)</li>
            <li>Not containing the letter: 0 points (Displayed as gray)</li>
            <li>Remaining time will be added as points after 5 guesses.</li>
        </ul>
    </li>
    <li>If a player makes the correct guess, the game is over, and that player is the winner.</li>
    <li>After the game ends, players can send a rematch request, and each match will include past game results.</li>
</ul>

<h2>Game Rules</h2>
<ul>
    <li>Only one game request can be sent at a time. When a game request is received, it must be accepted within 10 seconds; otherwise, the game will be declined.</li>
    <li>The word selection phase for players lasts 1 minute. If one player inputs a word but the other doesn't, the game will result in a win for the player who input a word.</li>
    <li>If a player disconnects during the game, they have 10 seconds to reconnect; otherwise, the game will be lost.</li>
</ul>

</body>
</html>
