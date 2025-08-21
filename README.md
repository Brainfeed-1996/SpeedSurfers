# Speed Surfers (Unity LTS, C#)

Version simplifiée de Subway Surfers: course automatique, 3 voies, obstacles, pièces, score et Game Over.

## Versions et dépendances
- Unity: dernière version LTS (2022/2023/6000 LTS)
- UI: TextMeshPro (inclus)

## Structure
- `Assets/Scripts/Player/PlayerController.cs`: déplacement joueur, input clavier + swipe, collisions
- `Assets/Scripts/Managers/GameManager.cs`: score, Game Over, restart
- `Assets/Scripts/Managers/UIManager.cs`: UI temps réel, écran fin
- `Assets/Scripts/Spawners/ObstacleSpawner.cs`: obstacles + sol infini
- `Assets/Scripts/Spawners/CoinSpawner.cs`: pièces et nettoyage

## Mise en place de la scène
1) Créer une scène `Main` et l'ajouter au Build Settings (index 0).
2) Créer un GameObject `GameManagerRoot` et ajouter `GameManager`.
3) UI:
   - Créer un `Canvas` (Screen Space - Overlay) + `EventSystem`.
   - Ajouter TextMeshPro si demandé (TMP Essential Resources).
   - Sur le `Canvas`, ajouter `UIManager`.
   - Ajouter:
     - `ScoreText` (TMP_Text) en haut-gauche. Lier dans `UIManager.scoreText`.
     - `GameOverPanel` (Panel désactivé par défaut) avec `FinalScoreText` (TMP_Text) et `RestartButton` (Button).
   - Référencer `gameOverPanel`, `finalScoreText`, `restartButton` dans `UIManager`.
4) Joueur:
   - Créer un GameObject `Player` (Capsule). Ajouter `PlayerController`.
   - Ajouter un `CapsuleCollider` (IsTrigger OFF).
   - (Optionnel) `Rigidbody` avec Gravity ON, Freeze Rotation X/Z.
   - Dans `PlayerController`:
     - `laneWidth` = 2.5 (doit correspondre aux spawners)
     - `cameraTransform` = `Main Camera`, ajuster `cameraOffset` (0,6,-8)
     - `obstacleTag` = `Obstacle`, `coinTag` = `Coin`
5) Caméra:
   - Utiliser `Main Camera`, assigner dans `PlayerController.cameraTransform`.
6) Sol/décor:
   - Créer un prefab `GroundSegment` (Cube étiré X=8, Y=0.1, Z=10). Sauvegarder dans `Assets/Prefabs`.
7) ObstacleSpawner:
   - Créer un GameObject `Spawners/ObstacleSpawner` et ajouter `ObstacleSpawner`.
   - Assigner `player` = `Player`.
   - `groundSegmentPrefab` = `GroundSegment`, `groundSegmentLength` = 10.
   - Préparer des prefabs d'obstacles (train court, barrière) avec `Collider` (IsTrigger OFF) + Tag `Obstacle` et les lier à `obstaclePrefabs`.
   - Réglages suggérés: `spawnDistanceAhead`=60, `despawnDistanceBehind`=25, `obstacleZStep`=8, `obstacleSpawnChancePerLane`=0.35.
8) CoinSpawner:
   - Créer `Spawners/CoinSpawner` et ajouter `CoinSpawner`.
   - Assigner `player` = `Player`.
   - Créer un prefab `Coin` (mesh simple), `Collider` IsTrigger ON, Tag `Coin`.
   - Assigner `coinPrefab`. Réglages: `coinZStep`=4, `coinSpawnChancePerLane`=0.5, `spawnTriplets`=ON.

## Tags et couches
- Créer Tag `Obstacle` et Tag `Coin`.
- Collisions par défaut. Les pièces doivent avoir IsTrigger = ON.

## Contrôles
- Clavier: Flèches Gauche/Droite (ou A/D) pour changer de voie.
- Mobile: swipe gauche/droite (seuil ~60 px).

## Logique de jeu
- Le joueur avance constamment (`forwardSpeed`).
- Changement de voie lissé vers X = [-laneWidth, 0, +laneWidth].
- Obstacles et sol générés devant, éléments trop loin détruits derrière.
- Pièces apparaissent aléatoirement; +1 score par pièce (désactivation pour pooling).
- Collision avec obstacle -> Game Over; panneau s'affiche; `Restart` recharge la scène.

## Assets gratuits recommandés
- Personnage/animations: `https://www.mixamo.com`
- Décor/props low poly: `https://kenney.nl/assets`
- Recherches Asset Store: "Free Low Poly City", "Simple Runner Pack", "Train", "Barrier".

## Conseils de perf
- Préférer pooling (désactiver/activer) aux instanciations massives.
- Optimiser matériaux (moins de variantes), light baking simple.

## Extensions possibles
- Power-ups: aimant à pièces, bouclier, double score, boost.
- Sauts/glissades, rails supplémentaires (4+ voies).
- Courbe de difficulté (vitesse/ densité dynamique).
- Missions quotidiennes, objectifs, boutique cosmétique.
- Post-processing, particules, audio SFX/musique.

## Raccordement rapide des scripts
- `PlayerController`: sur `Player`. Renseigner `cameraTransform`, tags et `laneWidth`.
- `GameManager`: sur `GameManagerRoot` (singleton persistant).
- `UIManager`: sur `Canvas`, relier `scoreText`, `gameOverPanel`, `finalScoreText`, `restartButton`.
- `ObstacleSpawner`: sur un objet de la scène, relier `player`, `groundSegmentPrefab`, `obstaclePrefabs`.
- `CoinSpawner`: sur un objet de la scène, relier `player`, `coinPrefab`.

## Dépannage
- Pièces non ramassées: vérifier Tag `Coin` + IsTrigger ON.
- Pas de Game Over: vérifier Tag `Obstacle` + colliders (non-trigger) ou triggers selon usage.
- Caméra figée: assigner `cameraTransform` dans `PlayerController`.
