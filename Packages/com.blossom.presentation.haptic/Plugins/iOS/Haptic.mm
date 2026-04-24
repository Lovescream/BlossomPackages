#import <UIKit/UIKit.h>

extern "C" {
    void _PlayHaptic(const char* type) {
        NSString* hapticType = [NSString stringWithUTF8String:type];

        if ([hapticType isEqualToString:@"Light"]) {
            UIImpactFeedbackGenerator* generator =
                [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
            [generator prepare];
            [generator impactOccurred];
        }
        else if ([hapticType isEqualToString:@"Medium"]) {
            UIImpactFeedbackGenerator* generator =
                [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
            [generator prepare];
            [generator impactOccurred];
        }
        else if ([hapticType isEqualToString:@"Heavy"]) {
            UIImpactFeedbackGenerator* generator =
                [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
            [generator prepare];
            [generator impactOccurred];
        }
        else if ([hapticType isEqualToString:@"Success"]) {
            UINotificationFeedbackGenerator* generator =
                [[UINotificationFeedbackGenerator alloc] init];
            [generator prepare];
            [generator notificationOccurred:UINotificationFeedbackTypeSuccess];
        }
        else if ([hapticType isEqualToString:@"Warning"]) {
            UINotificationFeedbackGenerator* generator =
                [[UINotificationFeedbackGenerator alloc] init];
            [generator prepare];
            [generator notificationOccurred:UINotificationFeedbackTypeWarning];
        }
        else if ([hapticType isEqualToString:@"Error"]) {
            UINotificationFeedbackGenerator* generator =
                [[UINotificationFeedbackGenerator alloc] init];
            [generator prepare];
            [generator notificationOccurred:UINotificationFeedbackTypeError];
        }
    }
}