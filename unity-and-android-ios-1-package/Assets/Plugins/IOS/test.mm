//
//  test.m
//  Unity-iPhone
//
//  Created by lixueliao on 2020/11/14.
//

#import <Foundation/Foundation.h>

extern "C"{
    
    void IOSLog(const char * message);
    
}

void IOSLog(const char * message){
    
    NSString* str = [[NSString alloc]initWithUTF8String:message];
    NSLog(str);
    
    UnitySendMessage("Canvas","IOSToUnity", str.UTF8String );
    
}
