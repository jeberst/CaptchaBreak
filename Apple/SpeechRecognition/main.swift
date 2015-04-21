//
//  main.swift
//  SpeechRecognition
//
//  Created by Guillermo Gomez on 4/12/15.
//  Copyright (c) 2015 Guillermo Gomez. All rights reserved.
//

import Foundation
import AppKit

println("Hello, World!")

class PrintRecognizer : NSObject, NSSpeechRecognizerDelegate {
    func speechRecognizer(sender: NSSpeechRecognizer, didRecognizeCommand command: AnyObject?){
        
        if command is String {
            println(command as! String)
        } else {
            println("didn't get it")
        }
        
        sender.stopListening()
    }
}

func getInput() -> String {
    var keyboard = NSFileHandle.fileHandleWithStandardInput()
    var inputData = keyboard.availableData
    return NSString(data: inputData, encoding:NSUTF8StringEncoding) as! String
}

func main() {
    
    var recognizer:NSSpeechRecognizer = NSSpeechRecognizer()
    
    recognizer.commands = ["test"]
    recognizer.delegate = PrintRecognizer()
    
    recognizer.startListening()
    println("listening")
}

main()
getInput()
println("ended")