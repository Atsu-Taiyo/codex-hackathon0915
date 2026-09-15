# codex robo — 3D平面マップと歩行

## 起動

Unity 6000.4.9f1で `Assets/_Project/Scenes/CodexRoboMap.unity` を開いてPlay。

- **WASD / 矢印キー**：カメラから見た方向に移動。
- **床を左クリック**：クリック位置まで歩いて停止。キー入力でクリック移動をキャンセル。
- 移動方向へ旋回し、左右の腕と脚を交互に振る。停止時は待機アニメーションへブレンド。
- 追従カメラ、外周の壁、衝突確認用の箱を配置済み。

## 他のマップで使う

`Assets/_Project/Content/Features/Map/CodexRobo/CodexRobo.prefab` をSceneに配置する。

1. コライダー付きの床を用意。クリック対象の床はLayer 8に設定するか、`CodexRoboMotor.groundMask` を床のLayerに合わせる。
2. カメラを `viewCamera` に指定。未指定なら `Camera.main` を使う。
3. 追従させる場合はカメラに `CodexRoboCamera` を付け、`target` をキャラクターに設定する。
4. 既存の入力処理から制御する場合は `acceptInput = false` とし、`WalkTo(worldPosition)`、`Steer(cameraRelativeDirection)`、`StopWalking()` を呼ぶ。

高さ1 unit、足元原点、前方向+Z、Y-up。`CharacterController` により地面・壁・障害物に衝突する。移動速度は `walkSpeed`、旋回速度は `turnSpeed` で調整可能。

## モデルとアニメーション

元のFBXと色テクスチャから、6本の骨（胴・頭・左右の腕・左右の脚）を持つ `SkinnedMeshRenderer` を生成。元のFBXの頂点形状・UVを保持し、専用の位置ベースのスキンウェイトを追加した。

- `Idle.anim`：2.4秒の待機ループ。
- `Walk.anim`：0.68秒の歩行ループ。腕と脚は逆位相、胴体の上下動と頭の小さな揺れを含む。
- `Animation` コンポーネントで再生するLegacy形式の独立したクリップ。Humanoidリターゲット用のリグではない。
- `CodexRobo_Skinned.asset`：派生メッシュ。元モデル同様約50万三角形（Unity上335,580頂点）。Git LFS対象。

短い一体型の手足を動かすための簡易リグで、指・肘・膝の独立制御や足裏IKは含まない。形状の異なるモデルにはウェイト生成条件の調整が必要。軽量化は未実施。

クリック移動は目的地への直進で、障害物の自動迂回（NavMesh経路探索）は含まない。障害物に当たったら別の床位置をクリックするかキーで移動する。

素材の利用条件は元モデルの `Assets/_Project/Content/Features/RoomOne/CloudRobot/SOURCE.md` を引き継ぐ。

## 再生成

**Tools > Codex Robo > Create Walking Prefab and Map** を実行する。

この操作は生成済みの歩行用メッシュ・マテリアル・クリップ・Prefabを更新する。元FBXは変更しない。既存の `CodexRoboMap.unity` は上書きせず、存在しない場合のみ生成する。Prefabやクリップを手編集する場合は別名のコピーを使う。

## 検証

MapのPlay中に **Tools > Codex Robo > Run Walking Playtest**。
結果は `Temp/CodexRobo-playtest.txt`、検証画像は `Temp/CodexRobo-*.png` に出力される。テスト終了時は開始地点・通常入力へ戻る。

専用コピーでのバッチ検証は `-batchmode -executeMethod Hackathon.Editor.CodexRoboBatchVerify.Run`。終了前に結果と画像をコピー側の `Verification/` に保存する。

### 実行結果（2026-09-15）

Unity 6000.4.9f1 / macOS Metalで33項目PASS。接地、目的地への到着・停止、左右の腕脚の動き、全17姿勢での脚の間隔、待機への遷移、斜め移動の速度、進行方向への旋回、外周と障害物との衝突を確認した。全周期のスキン変形後の頂点範囲はY=-0.00139〜1.01735 unit。

最終検証は同時作業と競合しない専用コピーで行い、検証済みメッシュ・Prefab・クリップを元の作業フォルダへ戻してSHA-256一致を確認した。Scene・Prefab・マテリアルの14個の参照GUIDも解決を確認済み。

- [テスト結果](CodexRobo/CodexRobo-playtest.txt)
- [資産のハッシュ](CodexRobo/verification.json)
- [待機姿勢](CodexRobo/CodexRobo-idle.png)
- [歩行姿勢1](CodexRobo/CodexRobo-walk-4.png) / [歩行姿勢2](CodexRobo/CodexRobo-walk-12.png)

専用コピーの起動時にUnity内部のQuickSearchインデックス初期化で `ArgumentOutOfRangeException` が出たが、C#コンパイル・Playテスト・画像出力は完了し、検証プロセスは終了コード0だった。手足は簡易スキニングで、極端なポーズへの対応や大量配置の性能検証は対象外。
