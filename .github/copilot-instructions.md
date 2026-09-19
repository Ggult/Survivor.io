# Repository Instructions

## Unity UI Layout Rules

When creating or modifying Unity UI, do not guess RectTransform values blindly. Preserve the existing UI system and make layout relationships explicit.

### Required order

1. Inspect the existing Canvas and parent hierarchy.
2. Create or select the parent.
3. Inspect the parent RectTransform.
4. Choose the child anchor preset.
5. Set the pivot.
6. Choose one sizing/positioning model.
7. Set size and position.
8. Re-read the resulting RectTransform from Unity and correct the layout if needed.

Parent UI elements by relationship, not by screen-coordinate assumptions.

### RectTransform conventions

For a full-screen element:

- `anchorMin = (0, 0)`
- `anchorMax = (1, 1)`
- `offsetMin = (0, 0)`
- `offsetMax = (0, 0)`

For a fixed-size element, keep `anchorMin == anchorMax`, then use `sizeDelta` and `anchoredPosition`.

- Center: anchor and pivot `(0.5, 0.5)`
- Top center: anchor and pivot `(0.5, 1)`; position is the offset from the top
- Bottom center: anchor and pivot `(0.5, 0)`; position is the offset from the bottom

Do not randomly mix `offsetMin`/`offsetMax` with `sizeDelta`/`anchoredPosition` on the same element. Use offsets for stretch layouts and `sizeDelta` plus `anchoredPosition` for fixed-size layouts, unless there is a documented reason otherwise.

### Layout constraints

- Create and inspect UI in the Unity Editor outside Play Mode.
- Do not change existing `CanvasScaler` settings without a concrete, verified reason.
- Do not hardcode a particular screen resolution such as `1080x1920`.
- Do not change parent RectTransforms to compensate for a child layout problem.
- Do not add Layout Groups, nested Layout Groups, `ContentSizeFitter`, or `LayoutElement` unless the layout genuinely requires them.
- Do not change a working UI hierarchy unnecessarily.
- Prefer a simple, explicit hierarchy for small groups such as three to five difficulty buttons.

### Responsive validation

After UI changes, validate the actual resulting layout at these aspect ratios:

- `1080x1920`
- `1080x2340`
- `1920x1080`

The UI must remain inside the screen and preserve its intended relative position. Do not report success from RectTransform numbers alone. Re-read the hierarchy and every important element after applying changes, and report:

- Parent
- Anchor Min
- Anchor Max
- Pivot
- Size Delta
- Anchored Position

### Scope

When fixing UI layout, do not modify gameplay logic or unrelated systems. Do not modify Player, Enemy, Combat, Pooling, or GameFlow gameplay code. UI layout changes only.

## Existing UI Repair Protocol

When asked to fix the scene's existing Unity UI, perform a repair pass only. Do not create a new gameplay system, redesign the UI architecture, or replace working UI scripts.

### Inspect before editing

Before changing any value, inspect the active scene and existing UI hierarchy in the Unity Editor. At minimum, inspect:

- Canvas and CanvasScaler
- GameHUD and Timer
- ResultPanel, ResultTitle, KillCountText, ReplayButton, and ReplayLabel
- Difficulty Selection UI and its Easy, Medium, and Hard buttons
- Other existing UI elements relevant to the layout problem

For every relevant element, read and record:

- Parent
- Anchor Min
- Anchor Max
- Pivot
- Size Delta
- Anchored Position
- Active state

Analyze the parent-child layout relationship before editing RectTransform values.

### Preserve the existing scene

- Keep the existing Canvas and CanvasScaler when they are working.
- Do not change Reference Resolution, Screen Match Mode, or Match Width Or Height merely to hide a child layout problem.
- Preserve the existing UI hierarchy and reuse existing GameObjects.
- Do not create unnecessary GameObjects, UI frameworks, Layout Groups, nested Layout Groups, `ContentSizeFitter`, or replacement scripts.
- Do not delete working UI scripts.

### Layout-specific repair expectations

- Full-screen overlays use stretch anchors and zero offsets.
- The Timer remains a fixed-size top-center element using top-center anchors and pivot.
- ResultPanel uses a full-screen overlay relationship when it is intended to cover the screen.
- ResultTitle uses a parent-relative center or top-center relationship.
- KillCountText is positioned relative to ResultPanel.
- ReplayButton is a fixed-size button positioned relative to ResultPanel.
- Existing Easy, Medium, and Hard buttons remain under their current parent, use consistent dimensions, and are aligned through parent-relative anchors.
- Never compensate for a child problem by changing the parent RectTransform.

### Validation and reporting

After each layout change, re-read the actual hierarchy and RectTransform values from Unity. Validate the visual result at `1080x1920`, `1080x2340`, and `1920x1080` when the environment allows it. Check that UI remains inside the screen, elements do not overlap, text is not clipped, buttons remain accessible, the Timer stays correctly positioned, the ResultPanel fills the screen, and difficulty buttons remain aligned.

Do not report a layout as correct from plausible numbers alone. If the Unity Editor or Game View cannot provide visual validation, report `NOT VALIDATED` rather than guessing.

The final report must state:

- Which existing UI elements were changed and the layout correction for each
- Whether CanvasScaler was changed
- Whether any new GameObject or Layout Group was added
- Whether gameplay code was changed
- Results for each requested aspect ratio using `PASS`, `FAIL`, or `NOT VALIDATED`
- Whether Console errors were found

During this repair protocol, do not modify GameFlow gameplay logic, PlayerMovement, PlayerRotation, PlayerAutoAttack, PlayerHealth, EnemyMovement, EnemyAttack, EnemyHealth, EnemyPool, EnemySpawner behavior, Combat, Animations, Muzzle Flash, Camera, or Arena systems. Change UI-side references only when the problem is strictly a UI reference or layout issue.

## TextMeshProUGUI Rules

An apparent TMP text problem is not necessarily an anchor problem. Evaluate each `TextMeshProUGUI` element as a combined hierarchy, parent, `RectTransform`, and TMP component problem.

### Required TMP inspection

For every relevant TMP element, inspect:

- Parent and active state
- RectTransform Anchor Min and Anchor Max
- Pivot
- Size Delta, width, and height
- Anchored Position
- Text alignment
- Text wrapping
- Overflow mode
- Font size
- Auto Size state

If a text element has a zero or implausibly small RectTransform width or height, fix that relationship before changing the font size. Text such as `SELECT DIFFICULTY` appearing one character per line is usually a narrow text RectTransform or wrapping issue, not a gameplay issue.

### Fixed-size centered text

For fixed-size centered text, use this as the starting relationship and adapt only to the real parent layout:

- `anchorMin = (0.5, 0.5)`
- `anchorMax = (0.5, 0.5)`
- `pivot = (0.5, 0.5)`
- Positive, meaningful width and height
- `anchoredPosition` relative to the parent
- Center / Middle alignment

For a title such as `SELECT DIFFICULTY`, a width near `600` and height near `100` may be a reasonable starting point, but the parent and actual text length determine the final size. Do not use screen coordinates as a substitute for parent-relative positioning.

### Wrapping and overflow

When TMP text unexpectedly wraps character-by-character or word-by-word, inspect in this order:

1. RectTransform width
2. RectTransform height
3. Text wrapping
4. Overflow mode
5. Alignment
6. Auto Size

Do not solve a layout problem only by shrinking the font. Short UI headings such as `SELECT DIFFICULTY` should not wrap vertically. Wrapping may be disabled and overflow may use `Overflow` when appropriate, but first ensure the RectTransform is wide enough and do not allow text to escape its parent blindly.

### Button child text

For a TMP label inside a button, the child text should normally fill the button:

- `anchorMin = (0, 0)`
- `anchorMax = (1, 1)`
- `offsetMin = (0, 0)`
- `offsetMax = (0, 0)`
- Center / Middle alignment
- Wrapping disabled for short button labels
- An overflow mode appropriate to the button bounds

The button may be fixed-size, while its child TMP text fills the parent. Do not give button labels unnecessary fixed pixel positions.

### Title text and Auto Size

Position title text relative to its UI panel, with sufficient width and height and center alignment. Do not use Auto Size as the default fix. Establish a correct RectTransform first; use Auto Size only when responsive text scaling is genuinely required, never to hide an incorrect layout.

### TMP validation checklist

For every TMP UI repair, verify:

- RectTransform width is greater than zero
- RectTransform height is greater than zero
- Anchor relationship is correct
- Pivot is correct
- Parent is correct
- Alignment is correct
- Wrapping has the intended behavior
- Overflow has the intended behavior
- Font size is reasonable
- Text is visually readable and does not overlap other UI

When validating existing TMP UI, inspect at minimum `SELECT DIFFICULTY`, Easy, Medium, Hard, Timer, Result Title, Kill Count, and Replay text. Re-read the actual RectTransform and TMP component values after editing, and report which TMP elements changed. Preserve the existing CanvasScaler, hierarchy, UI scripts, and gameplay systems unless a strictly UI-side reference or layout correction is required.
