# IW4M-Restart
Auto restart plugin for IW4M, works on Linux.
I decided that IW4M should not restart any game because it would be the parent of the newly created game (child) process.
Inside your shell script, you should have code to automatically detect when the game process was killed and restart it with Wine.