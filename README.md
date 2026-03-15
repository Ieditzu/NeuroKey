# NeuroKey monorepo

**Hackathon elevator pitch**  
NeuroKey turns neurofeedback into a family learning loop: kids play a Unity adventure instrumented with BCI events and coding puzzles; parents use a Kotlin app to set goals and see real‑time progress; a Spring Boot server syncs everyone over WebSockets and stores history in PostgreSQL. One repo, three coordinated apps, demoable in minutes.

## What’s inside
- `java-server/Java-Server` — Spring Boot 3.2 (Java 21) WebSocket + JPA backend on port `49154`; PostgreSQL persistence; AI/packet utilities.
- `kotlin-app` — Android app (Jetpack Compose, targetSdk 36) for parent auth, children/tasks/goals, QR login, AI learning profiles, theming.
- `unity/NeuroKey` — Unity 2022.3.62f3 URP project; first‑person island with coding/logic pads, optional g.tec BCI hooks, WebSocket client.

## Live demo in <10 minutes
1) Start the backend  
   ```bash
   cd java-server/Java-Server
   ./gradlew bootRun
   ```  
   Defaults: `jdbc:postgresql://localhost:5432/neuroKey`, user `kawase`, pass `root` (edit `src/main/resources/application.properties`).
2) Point clients at your server  
   - Android: update the URL in `SocketViewModel.connect(...)` if not using `wss://neuro.serenityutils.club`.  
   - Unity: set `serverUrl` on the `GameClient` component (inspector) or change it in code.
3) Run the clients  
   - Android: `./gradlew :app:installDebug` or run from Android Studio.  
   - Unity: open `unity/NeuroKey`, load `Assets/Scenes/bci.unity`, press Play.

## Why judges should care
- **BCI + gameplay**: hooks for g.tec EEG alongside classic controls; game events can react to attention/state signals.
- **Family loop**: parents set tasks/goals; kids earn points and AI learning profiles; both see updates instantly over WebSockets.
- **AI insight**: server aggregates interactions into strengths/struggles/mistakes summaries consumed by the app.
- **Resilient UX**: Android auto‑reconnects, hashes creds client‑side, and persists session prefs; Unity can run offline by disabling the network client.

## Architecture snapshot
- **Transport**: Binary WebSockets with a shared packet schema across Unity, Android, and Java server.
- **Backend**: Spring Boot + JPA + Hypersistence utilities; services for parents/children/tasks/goals/game sessions; QR login flow; global tasks seeded on boot.
- **Clients**:  
  - Unity: `Assets/Scripts/Runtime/...` handles pads, portals, cinematics, mobile UI helpers; `GameClient` singleton manages the socket.  
  - Android: `SocketViewModel` drives state; Compose screens for auth, dashboard, child/task/goal lists, AI profiles, dark mode, color picks.

## Tooling checklist
- Java 21+ (toolchain configured) and bundled Gradle wrapper.
- PostgreSQL reachable with the creds above or your overrides.
- Android Studio with SDK 36; device/emulator for the app.
- Unity Hub with **2022.3.62f3**; URP preconfigured (check Project Settings → Graphics if rendering looks off).

## Fast iteration
- Backend tests: `./gradlew test` in `java-server/Java-Server`.
- Android sanity: `./gradlew :app:connectedAndroidTest` (device/emulator).
- Unity play mode: toggle the `GameClient` object to switch online/offline runs.

## Talking points to use
- Real‑time triad (game + parent app + server) over one protocol.
- Educational gameplay (coding/logic pads) blended with neurofeedback for engagement.
- Privacy by design: client‑side credential hashing; WebSocket channel only.
- Extensible: swap BCI provider, add packet types, retheme Unity scenes without changing the protocol.

## Contributing
Open issues/PRs per component. Keep third‑party asset licenses intact (Unity art packs, g.tec SDK, Starter Assets). Avoid committing secrets; override creds via local configs.***
