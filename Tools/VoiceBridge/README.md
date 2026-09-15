# Room One voice cards (macOS)

Unity の Mic ボタン → macOS Speech → **codex-component** → Codex app server → 検証済みカードを一括反映。

GitHub の [Atsu-Taiyo/codex-component](https://github.com/Atsu-Taiyo/codex-component) を commit `430c25ff4d77e7400ff379603b452f8417df45bb` に固定して使用します。独自の app-server 通信や MCP サーバーは実装していません。

## 起動

Node.js 22 以上、Codex CLI、Xcode Command Line Tools、macOS が必要です。

```sh
cd Tools/VoiceBridge
npm ci --allow-git=root
sh build-speech.sh
codex login # 未ログインの場合のみ
npm start
```

Unity の Bootstrap シーンを Play。ロボットと箱をクリックして発見後、**Mic** を押して「箱がロボットを持ち上げる」「ロボットと箱を入れ替えて」などと話します。約 1.8 秒の無音で確定し、最大 15 秒録音します。初回は macOS のマイク・音声認識の許可が必要です。音声は macOS Speech の設定と提供状況により Apple に送信される場合があります。認識テキストとカード状態を Codex に送信します。音声ファイルは保存しません。

カードが配置されたら RUN で実験。Stop で取消。処理中にカードを変更した場合は、その結果を適用しません。未発見語・重複語・不正な応答も拒否します。認識不能・不明瞭な指示は再試行してください。ChatGPT ログインを使い、追加の OpenAI API key は不要です。

このマイク実装は macOS Editor / macOS standalone 向けです。WebGL / Windows は未対応。ブリッジは同じ Mac で起動してください。ポートは loopback `47831` 固定です。ブラウザーからのリクエストは受け付けません。

## 検証

```sh
npm test
curl -s http://127.0.0.1:47831/health
curl -s http://127.0.0.1:47831/arrange \
  -H 'Content-Type: application/json' \
  -d '{"cards":["robot","lift","box"],"vocabulary":["robot","box","lift"],"text":"ロボットと箱を入れ替えて"}'
```

`/arrange` は録音を省いた実接続の検証用。実際のボタンは `/listen` を使います。エラー時はカードを保持します。Codex CLI の場所は `CODEX_BIN` 環境変数で指定できます。
