#import <UIKit/UIKit.h>

extern "C" void LensNativeShare_Show(const char *subject, const char *text)
{
    NSString *subjectString = subject ? [NSString stringWithUTF8String:subject] : @"Lens Debug Report";
    NSString *textString = text ? [NSString stringWithUTF8String:text] : @"";

    dispatch_async(dispatch_get_main_queue(), ^{
        UIViewController *root = UIApplication.sharedApplication.keyWindow.rootViewController;
        if (root == nil)
        {
            return;
        }

        UIActivityViewController *controller = [[UIActivityViewController alloc] initWithActivityItems:@[textString] applicationActivities:nil];
        [controller setValue:subjectString forKey:@"subject"];

        if (controller.popoverPresentationController != nil)
        {
            controller.popoverPresentationController.sourceView = root.view;
            controller.popoverPresentationController.sourceRect = CGRectMake(root.view.bounds.size.width * 0.5f, root.view.bounds.size.height * 0.5f, 1.0f, 1.0f);
        }

        [root presentViewController:controller animated:YES completion:nil];
    });
}
