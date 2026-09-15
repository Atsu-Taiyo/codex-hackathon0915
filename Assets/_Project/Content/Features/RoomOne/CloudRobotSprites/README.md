# CloudRobot：シーン別アニメーション素材

CloudRobotの質感で生成した2枚の画像を、Room Oneの7シーン・21ポーズに分割しました。あわせて、ロボットと箱それぞれの単体待機画像も2枚用意しています。

[全21ポーズの一覧](../../../../../../ArtSource/Features/RoomOne/CloudRobotSprites/scene-contact-sheet.png) · [再生プレビュー](../../../../../../ArtSource/Features/RoomOne/CloudRobotSprites/preview.html) · [フレーム定義](animation-manifest.json)

`preview.html` は、ダウンロードしたリポジトリ内からブラウザで開いてご確認ください。GitHub上ではHTMLのソースコードが表示されてしまいます。

## シーンと再生順

各フォルダの `01 → 02 → 03` を、1枚あたり700msで表示します。既存のRoom Oneと同じ3コマ方式です。

| フォルダ（Frames内） | シーンID | 用途 | 01 → 02 → 03 |
| --- | --- | --- | --- |
| RobotPushBox | `robot:push:box` | ロボットが箱を押す | 準備 → 押す → 押し終える |
| RobotPullBox | `robot:pull:box` | ロボットが箱を引く | 準備 → 引く → 引き終える |
| RobotLiftBox | `robot:lift:box` | ロボットが箱を持ち上げる | 準備 → 持ち上げる → 成功 |
| RobotOpenBox | `robot:open:box` | ロボットが箱を開ける | 閉じた箱 → ふたを開ける → 開いた箱 |
| RobotShakeBox | `robot:shake:box` | ロボットが箱を振る | 持つ → 左へ振る → 右へ振る |
| RobotBreakBox | `robot:break:box` | ロボットが箱を壊す | 準備 → 衝撃 → 壊れた箱 |
| BoxLiftRobot | `box:lift:robot` | 箱がロボットを持ち上げる | 準備 → 持ち上げる → 頭上へ |

「引く」という名称は、既存の `RoomOneRules.Actions` と元アトラスの2段目に基づいています。押す動作と引く動作の絵は似ているため、ファイル名とシーンIDで区別しています。

## 単独のキャラクター

- `Characters/RoomOne_CloudRobot_RobotIdle.png`：ロボット単体の待機姿勢です。
- `Characters/RoomOne_CloudRobot_BoxIdle.png`：通常の箱単体です。

接触や重なりのある21ポーズについては、ロボットと箱をセットで切り出しています。隠れている身体の描き足しや、身体パーツへの分解は行っていません。

## Unityでの使い方

1. `git lfs pull` でPNGを取得します。
2. 各PNGをSpriteとして取り込むための `.meta` ファイルを付属させています。設定は1ファイル1Sprite、100 pixels/unit、中心pivot、Bilinear、圧縮なし、mipmapなしです。
3. 用途に合うフォルダの3枚を、UI ImageやSpriteRendererなどで順番に表示します。元画像の切り出し位置やキャンバス内の配置は `animation-manifest.json` に記載しています。

標準の6シーンは418×245px、箱が持ち上げるシーンは1024×561pxです。同じシーン内でサイズを揃え、余白を透明にして下揃えで配置しています。元のRGBAピクセルは拡大縮小や色変更をせず、そのまま保持しています。

これらは2Dのポーズ素材です。3D FBXのリグやアニメーション、UnityのAnimator Clipは含みません。また、既存ゲームで使っているアトラスの差し替えや再生コードへの接続は、今回の追加作業には含まれていません。

## 検証と制限

- 23枚のPNGを読み込み、RGBA形式であること、画像の寸法、可視ピクセルの存在を確認しました。
- 各切り出し画像のRGBAが、元画像の対応領域と一致していることを確認しました。
- 全21ポーズを背景に合成した一覧で、場面の対応やキャラクターが切れていないことを確認しました。
- 元画像には輪郭付近に微細なノイズがあります。透明部分にRGBが残っていますが、通常のアルファ合成では表示されません。
- Unity Editorでのインポートやゲーム内での再生は未検証です。プレビューのブラウザ表示についても確認環境が利用できないため、静的検査のみにとどまっています。

元画像、生成プロンプト、再分割スクリプトは `ArtSource/Features/RoomOne/CloudRobotSprites/` にあります。分割にはPythonとPillowを使用します。リポジトリ直下から以下を実行すると再生成できます。

```sh
python3 ArtSource/Features/RoomOne/CloudRobotSprites/split_sheets.py
```

出典については [SOURCE.md](SOURCE.md) をご確認ください。
