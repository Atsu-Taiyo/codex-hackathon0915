import http from 'node:http';
import { execFile } from 'node:child_process';
import { fileURLToPath } from 'node:url';
import { arrange } from './codex.mjs';
import { validateInput } from './cards.mjs';
const speech = fileURLToPath(new URL('./build/Room One Voice.app/Contents/MacOS/RoomOneSpeech', import.meta.url));
let busy = false;
http.createServer(async (req, res) => {
  const json = (status, value) => { if (!res.destroyed) { res.writeHead(status, { 'Content-Type': 'application/json' }); res.end(JSON.stringify(value)); } };
  // Native Unity only. Block browser origins and DNS rebinding.
  if (req.headers.origin || !['127.0.0.1:47831', 'localhost:47831'].includes(req.headers.host)) return json(403, { error: 'Native loopback requests only' });
  if (req.url === '/health' && req.method === 'GET') return json(200, { ok: true });
  if (!['/listen', '/arrange'].includes(req.url) || req.method !== 'POST') return json(404, { error: 'Not found' });
  if (busy) return json(409, { error: 'Microphone or Codex is busy. Try again.' });
  busy = true;
  const controller = new AbortController();
  res.on('close', () => controller.abort());
  const timer = setTimeout(() => controller.abort(), 85000);
  try {
    let body = '';
    for await (const part of req) { body += part; if (body.length > 8192) throw new Error('Request too large'); }
    const input = validateInput(JSON.parse(body));
    if (req.url === '/listen') {
      input.text = await new Promise((resolve, reject) => {
        execFile(speech, [], { timeout: 22000, signal: controller.signal, maxBuffer: 65536 }, (error, stdout) => {
          let result; try { result = JSON.parse(stdout); } catch { reject(new Error('Speech helper unavailable. Run sh build-speech.sh; allow microphone and speech recognition in System Settings.')); return; }
          if (error || result.error || !result.text?.trim()) reject(new Error(result.error || 'No speech detected'));
          else resolve(result.text);
        });
      });
    }
    if (!input.text) throw new Error('Transcript required');
    json(200, await arrange(input, controller.signal));
  } catch (error) { json(400, { error: error.message }); }
  finally { clearTimeout(timer); busy = false; }
}).listen(47831, '127.0.0.1', () => console.log('Room One Voice ready at 127.0.0.1:47831'));
