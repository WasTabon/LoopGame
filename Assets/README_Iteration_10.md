# Loop Puzzle — Итерация 10: Финальный проход

Финальная сборка и полировка. Игра закрывается как цельный продукт: меню → выбор уровней → 25 уровней по 5 мирам → экран завершения. Спецмеханики Мира 5 — отдельными итерациями позже.

## Что нового в этой итерации

- **Кнопка паузы** в правом верхнем углу во время игры. Открывает попап паузы: RESUME / RESTART / MAIN MENU. Теперь из уровня можно выйти не только через победу.
- **Экран завершения игры** — после прохождения последнего (25-го) уровня показывается попап «All Levels Complete!» со сводкой собранных звёзд (X из 75).
- **Кнопка победы на финале** — на последнем уровне кнопка NEXT превращается в FINISH и ведёт на экран завершения (раньше просто пряталась).
- Подсчёт суммы звёзд по всем уровням (`ProgressManager.GetTotalStars`).

## Установка (поверх итерации 9)

1. Нужны выполненные Setup итераций 1–9.
2. Скопируй `Assets/LoopPuzzle/` поверх итерации 9.
3. Запусти `Tools → Loop Puzzle → Setup Iteration 10`.

Setup добавит в игровую сцену кнопку паузы, попап паузы и попап завершения игры, прокинет ссылки в `HUDController` и `LevelController`, и привяжет лейбл кнопки NEXT (для переключения на FINISH).

---

# Полная сборка проекта с нуля

Если собираешь игру в чистом Unity-проекте, выполни Setup-команды по порядку. Каждая итерация опирается на предыдущие.

## Требования
- Unity 2021+ (2D).
- Импортированы **DOTween** (бесплатный, Asset Store) и **TMP Essentials** (`Window → TextMeshPro → Import TMP Essential Resources`).

## Порядок установки
Скопируй папку `Assets/LoopPuzzle/` в проект, затем выполни по очереди в меню `Tools → Loop Puzzle`:

1. **Setup Iteration 1** — фундамент: сцены MainMenu и Game, менеджеры (звук, гаптика, переходы), базовый UI меню.
2. **Setup Iteration 2** — сетка, элементы пути, поворот тапом (ядро Мира 1).
3. **Setup Iteration 3** — проверка победы, HUD, попап победы.
4. **Setup Iteration 4** — анимация потока по замкнутой петле.
5. **Setup Iteration 5** — перетаскивание элементов (ядро Мира 2).
6. **Setup Iteration 6** — 25 уровней, база уровней, экран выбора (скролл по мирам), прогрессия, сохранения, звёзды.
7. **Setup Iteration 7** — Undo / Restart / Hint + настройки.
8. **Setup Iteration 8** — туториал через подсветку (уровни 1 и 6 + плашки на новых мирах).
9. **Setup Iteration 9** — визуальная полировка: спрайты звёзд/замка, свечение элементов, частицы, каскад появления.
10. **Setup Iteration 10** — пауза, экран завершения игры, финальная сборка.

После итерации 6 в Build Settings автоматически добавляются 3 сцены: MainMenu, LevelSelect, Game.

## Полезные editor-команды
- `Tools → Loop Puzzle → Validate All Levels` — проверяет проходимость и уникальность всех уровней.
- `Tools → Loop Puzzle → Unlock All Levels` — открыть все уровни.
- `Tools → Loop Puzzle → Lock All Levels (reset to 1)` — заблокировать и сбросить прогресс.

---

# Структура проекта

```
Assets/LoopPuzzle/
├── Scenes/           MainMenu, LevelSelect, Game
├── Scripts/
│   ├── Core/         GameBootstrap, GameSession
│   ├── Data/         LevelData, LevelDatabase (ScriptableObjects)
│   ├── Gameplay/     GridManager, Cell, PathPiece, PieceInput, PieceConnections,
│   │                 LoopValidator, LevelValidator, LevelController,
│   │                 LoopPathBuilder, LoopFlowAnimator, CameraShake,
│   │                 PieceSpriteFactory, MoveHistory, HintSystem, TutorialController
│   ├── Managers/     SoundManager, HapticManager, TransitionManager, ProgressManager
│   └── UI/           MainMenuController, HUDController, WinPopup, PopupBase,
│                     LevelSelectController, LevelButton, StarDisplay, SettingsPopup,
│                     TutorialBanner, PausePopup, GameCompletePopup, ButtonPunch, SafeAreaFitter
├── Editor/           Iteration1–10 Setup, LevelValidatorMenu, UnlockAllLevelsMenu,
│                     LevelData/levels_export.json (проверенные данные 25 уровней)
└── Levels/           25 ассетов LevelData (создаются Setup 6)
```

---

# Геймплей и контент

**Цель:** поворачивать (тап) и перетаскивать элементы, чтобы собрать замкнутую петлю (в Мире 4 — две петли).

**5 миров по 5 уровней (25 всего), сложность растёт:**
- **Мир 1 (1–5)** — обучение, только поворот. Формы: квадраты, L.
- **Мир 2 (6–10)** — препятствия, добавляется перетаскивание.
- **Мир 3 (11–15)** — зафиксированные элементы (нельзя двигать, отличаются цветом).
- **Мир 4 (16–20)** — нужно собрать две отдельные петли.
- **Мир 5 (21–25)** — большие поля, Triple/Cross, фигура-8.

Поле растёт 4×4 → 8×8, элементов 4 → 20. Все уровни проверены на проходимость и уникальность двумя независимыми методами (построение решения + перебор).

**Звёзды (0–3):** пройден (+1), без подсказок (+1), уложился в Par по ходам (+1).

---

# Что дальше (отдельными итерациями)

Игра готова как цельный продукт. Усложнение — отдельно:
- Спецмеханики Мира 5: **Color Loops** (одноцветные петли), **Loop Size** (петля ровно из N), **One Move Challenge** (решить за 1 ход).
- Возможный процедурный генератор уровней.
- Полировка звука/музыки (генеративный эмбиент, более богатые эффекты).
- Дополнительный контент (больше уровней/миров).

Каждую из этих фишек лучше делать отдельной итерацией с собственным контентом и валидацией.
