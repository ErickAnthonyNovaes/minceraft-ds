# MinceraftAPI — Deploying to Render (Docker)

This repository contains a .NET 9 ASP.NET application. The project includes a `Dockerfile` ready for use on Render or other container platforms.

Quick summary
- The app serves `wwwroot/index.html` at `/` and provides a SPA fallback.
- Static files are in `wwwroot` and the protected content is served by `GET /conteudo`.
- API endpoints: `/Bloqueio/Estado`, `/Bloqueio/Alternar` and SignalR hub at `/hub/bloqueio`.
- Health endpoint: `GET /health` returns `{ "status":"ok" }`.

Deploying on Render (Docker)
1. Push this repo to a Git provider (GitHub/GitLab/Bitbucket).
2. Create a new Web Service on Render and connect your repo.
3. Choose "Docker" as the environment and set the build context to the repository root (where the `Dockerfile` sits).
4. Render will build the image using the provided `Dockerfile`. The container listens on port 80 inside; Render injects a runtime `$PORT` — the Dockerfile will use it if provided.
5. (Optional) Configure a health check in Render to poll `/health` (HTTP 200 expected).

Notes and tips
- If you prefer explicit dynamic port binding, the Dockerfile already respects `$PORT` with a fallback to 80.
- TLS is terminated by Render; your app should listen on HTTP inside the container.
- If you need to customize environment variables (for example a `SwaggerSecret`), set them in Render's environment settings.

Local testing with Docker
```powershell
docker build -t minceraftapi:local .
docker run --rm -p 8080:80 --name minceraftapi minceraftapi:local
```

Open http://localhost:8080 to see the app. `/health` should return a small JSON object.

If you want, I can add a Render-specific `render.yaml` or tune the Dockerfile further (multi-stage caching optimizations, smaller base images, etc.).
