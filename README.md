# SharpNukeEventLog
nuke that event log using some epic dinvoke fu

Inspired by https://www.ired.team/offensive-security/defense-evasion/disabling-windows-event-logs-by-suspending-eventlog-service-threads and 
https://github.com/hlldz/Invoke-Phant0m

in order for this to compile you'll have to add `System.Management` to your refferences, which should be found here: `C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Management.dll` 

Tested for x64 systems, pretty sure it wont work for x86 unless you do some magic with the IntPtr marshalling.

For red teamer, by a red teamer. 
I will not take part in the whole OST debate. 


```
              /\                       |\**/|
             /  \                      \ == /
             |  |                       |  |
             |  |     EventlogNuker     |  |
            / == \       @jfmaes        \  /
            |/**\|                       \/



target found, nuke launched on the eventlog threads of PID: 1380
wevtsvc.dll found at 0x140733035708416
suspending eventlog thread 2204
suspending eventlog thread 2564
suspending eventlog thread 2568
suspending eventlog thread 2580


                  _.-^^---....,,--
             _--                  --_
            <                        >)
            |                         |
             \._                   _./
               ```--. . , ; .--'''
                     | |   |
                  .-=||  | |=-.
                  `-=#$%&%$#=-'
                     | ;  :|
            _____.,-#%&$@%#&#~,._____

        Eventlog nuked successfully!
        
```
