#import <AVFoundation/AVFoundation.h>

// Sets the iOS audio session to the Playback category: plays through the main
// speaker at full volume and ignores the physical silent/mute switch, without
// requiring microphone access. Called from ViewManager at startup (and on resume).
extern "C" void _ConfigureAudioSessionPlayback() {
    NSError *error = nil;
    AVAudioSession *session = [AVAudioSession sharedInstance];
    [session setCategory:AVAudioSessionCategoryPlayback error:&error];
    [session setActive:YES error:&error];
}
