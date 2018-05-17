#if TARGET_OS_IOS

#import "UnityAppController.h"
#import <TargetConditionals.h>

extern void UnityPluginLoad(struct IUnityInterfaces *interfaces);
extern void UnityPluginUnload(void);

#pragma mark - App controller subclasssing

@interface MyAppController : UnityAppController
{
}
- (void)shouldAttachRenderDelegate;
@end

@implementation MyAppController
- (void)shouldAttachRenderDelegate;
{
    UnityRegisterRenderingPluginV5(&UnityPluginLoad, &UnityPluginUnload);
}
@end

IMPL_APP_CONTROLLER_SUBCLASS(MyAppController);

#endif
