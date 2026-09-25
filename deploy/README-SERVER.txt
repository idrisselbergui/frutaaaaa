FRUTA - SERVER GUIDE (Docker)
=============================

Folder on the server: C:\FrutaDocker
Address: http://localhost:5005 on the server, http://192.168.1.200:5005 from other PCs
Run all commands in PowerShell inside C:\FrutaDocker.


FIRST INSTALL
-------------
1. Copy the zip (fruta-deploy-....zip) to the server.
2. Right-click > Extract All... > C:\FrutaDocker
3. docker compose up -d --build
4. Open http://localhost:5005 and log in.


UPDATE TO A NEW VERSION
-----------------------
1. Keep the previous zip (to go back if needed).
2. Delete the folder C:\FrutaDocker\app  (your .env file is not touched)
3. Extract the new zip into C:\FrutaDocker and replace files when asked.
4. docker compose up -d --build
5. docker image prune -f      (removes the old image, frees disk space)

To go back to the previous version: same steps with the previous zip.


TIME CHECK
----------
The app must use the same time as Windows. Check with:
  docker exec fruta date
It must show the same hour as the Windows clock. If Windows changes its offset
(e.g. Ramadan / summer change), edit TZ in docker-compose.yml
(UTC = UTC+0, Etc/GMT-1 = UTC+1) and run: docker compose up -d


USEFUL COMMANDS
---------------
docker compose ps                    status (should say "Up")
docker compose logs -f --tail 100    live logs (Ctrl+C to stop watching)
docker compose restart               restart the app
docker compose down                  stop the app
type VERSION.txt                     which version is installed
