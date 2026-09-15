import Foundation
import Speech
import AVFoundation

let engine = AVAudioEngine()
let request = SFSpeechAudioBufferRecognitionRequest()
var task: SFSpeechRecognitionTask?
var finished = false
var latest = ""
var silence: Timer?
func finish(_ text: String, _ error: String = "") {
    if finished { return }; finished = true
    engine.stop(); request.endAudio(); task?.cancel()
    let data = try! JSONSerialization.data(withJSONObject: ["text": text, "error": error])
    print(String(data: data, encoding: .utf8)!); fflush(stdout)
    exit(error.isEmpty ? 0 : 1)
}
func start() {
    guard let recognizer = SFSpeechRecognizer(locale: Locale(identifier: "ja-JP")), recognizer.isAvailable else {
        finish("", "Speech recognition is unavailable."); return
    }
    recognizer.queue = OperationQueue.main
    request.shouldReportPartialResults = true
    let input = engine.inputNode
    let format = input.outputFormat(forBus: 0)
    guard format.sampleRate > 0 else { finish("", "No microphone available."); return }
    input.installTap(onBus: 0, bufferSize: 1024, format: format) { buffer, _ in request.append(buffer) }
    task = recognizer.recognitionTask(with: request) { result, error in
        if let result = result {
            latest = result.bestTranscription.formattedString
            silence?.invalidate()
            if result.isFinal { finish(latest); return }
            silence = Timer.scheduledTimer(withTimeInterval: 1.8, repeats: false) { _ in finish(latest) }
        }
        if let error = error { finish("", error.localizedDescription) }
    }
    do { engine.prepare(); try engine.start() } catch { finish("", error.localizedDescription) }
    Timer.scheduledTimer(withTimeInterval: 15, repeats: false) { _ in
        finish(latest, latest.isEmpty ? "No speech detected. Try again." : "")
    }
}
SFSpeechRecognizer.requestAuthorization { status in
    guard status == .authorized else { finish("", "Allow Speech Recognition for Room One Voice in System Settings."); return }
    AVCaptureDevice.requestAccess(for: .audio) { granted in
        DispatchQueue.main.async {
            if granted { start() } else { finish("", "Allow Microphone for Room One Voice in System Settings.") }
        }
    }
}
RunLoop.main.run()
