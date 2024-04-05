# Sample Visual Studio Extension using VS SDK and VisualStudio.Extensibility

This is a small sample extension that was put together to see both VS SDK and VisualStudio.Extensibility (VS.Ext) extension entrypoints working together. This mostly followed the guidance [Using VisualStudio.Extensibility SDK and VSSDK together](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/get-started/in-proc-extensions?view=vs-2022).

This extension has two menu commands, each showing a "hello world" message. The VS SDK based command uses the VSCT file to integrate with the Extensions menu. The VS.Ext based command self-declares that it is added to the Extensions menu.

## Conclusions

It is possible to have an existing VS SDK based extension, and leverage the very simple approach to adding commands and menus provided by VS.Ext. This is a plausible milestone for VS SDK extensions where a migration may require a longer timespan.

## Journey

To get to a state where I had both extensibility models working together, I started with a VS SDK extension, and tried to incrementally convert it to support VS.Ext. The [commit history](https://github.com/awschristou/VsSampleHybridExtension/commits/main/) contains this journey. Here was my journey:

### Start with a working VS SDK Extension

I started by creating a VS SDK extension from scratch ([6c929ccb](https://github.com/awschristou/VsSampleHybridExtension/commit/6c929ccb51682fa8e7601a7318226a26db44a93d)). The extension auto-loads, and contains one menu command, which shows a message box.

### Convert the project to SDK stype project

From [Add a VisualStudio.Extensibility extension to an existing VSSDK extension project](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/get-started/in-proc-extensions?view=vs-2022#add-a-visualstudioextensibility-extension-to-an-existing-vssdk-extension-project):

> You need an SDK style .csproj in order to utilize VisualStudio.Extensibility SDK packages. For existing projects, you might need to update your .csproj to an SDK style one.

I next tried to perform an in-place conversion, updating the csproj file to an SDK style project ([361f5d05](https://github.com/awschristou/VsSampleHybridExtension/commit/361f5d0513135d5782a94cc1bb1d6d4ca61ecf65)).

There is not a good set of instructions around what this looks like for VSIX based projects. I was able to convert the project to something that compiles, however it doesn't produce a VSIX. It would be great if the documentation could provide more instructions, and a working sample of an SDK-style project containing a VS SDK extension.

I proceeded even though I was unable to see this working.

### Add a VS.Ext entrypoint and contribution point

Now that I have an SDK style project, I followed the additional steps in [Add a VisualStudio.Extensibility extension to an existing VSSDK extension project](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/get-started/in-proc-extensions?view=vs-2022#add-a-visualstudioextensibility-extension-to-an-existing-vssdk-extension-project) ([d9261020](https://github.com/awschristou/VsSampleHybridExtension/commit/d9261020c4be3ba562194eb2ba15f2e86011f704)). I added a VS.Ext based command that shows a message box.

I did update [source.extension.vsixmanifest](https://github.com/awschristou/VsSampleHybridExtension/commit/d9261020c4be3ba562194eb2ba15f2e86011f704#diff-d2938df1c5b2f08648372307c3f5905d57b4fc599ffd26d287018baf6627784fR8) to set `Installation ExtensionType="VSSDK+VisualStudio.Extensibility"`, which may be missing from the documentation.

With these changes, a VSIX is produced, and gets installed into the VS Experimental instance. I am able to successfully call both the VS SDK and VS.Ext commands.

### Use the VS.Ext API from VS SDK extension code

Now that I have both extensibility entry points, I'll want to know that I can leverage the cleaner VS.Ext API from legacy VS SDK extensions. I followed the guidance in [Use VisualStudio.Extensibility from existing VSSDK extensions](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/get-started/in-proc-extensions?view=vs-2022#use-visualstudioextensibility-from-existing-vssdk-extensions) to try accessing the VS.Ext from my VS SDK based command ([cc1d1326](https://github.com/awschristou/VsSampleHybridExtension/commit/cc1d1326ff1eed78200b5aadc8fad82e096bb786)).

This wasn't successful. It appears the VisualStudioExtensibility service is not available.

## Problems

### Discovery / Documentation

The Journey section above shows some rough edges that will make it challenging for developers to onboard existing extensions to VS.Ext:

- converting VSIX csproj files to a SDK style project can be very challenging and time consuming
- I am concerned that my existing extension's stability may be compromised as a result of the SDK-style project conversion. There's a lot that goes on in the VS build tooling that I don't understand.
- the docs may be missing some steps/content around `Installation ExtensionType="VSSDK+VisualStudio.Extensibility"` in the vsixmanifest file

### Unable to obtain the VisualStudioExtensibility service

With guidance from [Use VisualStudio.Extensibility from existing VSSDK extensions](https://learn.microsoft.com/en-us/visualstudio/extensibility/visualstudio.extensibility/get-started/in-proc-extensions?view=vs-2022#use-visualstudioextensibility-from-existing-vssdk-extensions), I should be able to gain access to the VS.Ext API from a VS SDK based extension, however this results in the exception "The VisualStudioExtensibility service is unavailable."

The sample code can be seen in [VsSdkHelloWorldCommand.cs](https://github.com/awschristou/VsSampleHybridExtension/commit/cc1d1326ff1eed78200b5aadc8fad82e096bb786#diff-d54a7b54fc8535f87c248ffbd8fc2c45d371792a79b37c81eb23f1d36c399b14R104).

### VS.Ext Command can fail shortly after start-up

_Update: This will be fixed in an [upcoming 17.10 preview](https://github.com/microsoft/VSExtensibility/issues/350)_

If I try to run the VS.Ext based command immediately after Visual Studio starts up, nothing happens. My debugger output contains the following:

```
RemoteCommandsLog Information: 0 : Attempting to load service 'HybridExtension.ExtensionEntrypointCommandSet (1.0)' for command set 'HybridExtension.ExtensionEntrypoint'
The thread '[Thread Destroyed]' (14308) has exited with code 0 (0x0).
'devenv.exe' (CLR v4.0.30319: DefaultDomain): Loaded 'C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\PrivateAssemblies\Octokit.dll'. Skipped loading symbols. Module is optimized and the debugger option 'Just My Code' is enabled.
RemoteCommandsLog Error: 0 : Failed to load service 'HybridExtension.ExtensionEntrypointCommandSet (1.0)'

InternalErrorException: Cannot find an instance of the Microsoft.VisualStudio.RpcContracts.Commands.ICommandSetProvider service.

Microsoft.Assumes.Fail(String message)
Microsoft.Assumes.Present[T](T component)
Microsoft.VisualStudio.Commands.Client.Models.CommandSetModel.<EnsureCommandSetAsync>d__52.MoveNext()

HResult: 0x80131500

RemoteCommandsLog Error: 0 : Error executing command HybridExtension.ExtensionEntrypoint:HybridExtension.VsExtHelloWorldCommand (HYBRID SAMPLE: Hello World (VS.Ext))

InvalidOperationException: Failed to activate service for command execution.

Microsoft.VisualStudio.Commands.Client.Models.CommandSetModel.<>c__DisplayClass56_0.<<ExecuteCommandAsync>b__0>d.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
Microsoft.VisualStudio.Threading.ReentrantSemaphore.NotAllowedSemaphore.<>c__DisplayClass2_0.<<ExecuteAsync>b__0>d.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
Microsoft.VisualStudio.Threading.ReentrantSemaphore.NotAllowedSemaphore.<ExecuteAsync>d__2.MoveNext()
--- End of stack trace from previous location where exception was thrown ---
System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()
System.Runtime.CompilerServices.TaskAwaiter.HandleNonSuccessAndDebuggerNotification(Task task)
Microsoft.VisualStudio.Commands.Client.Models.CommandSetModel.RemoteCommandModel.<ExecAsync>d__26.MoveNext()

HResult: 0x80131509
```

### SDK Style support for vsct files

My initial VS SDK based extension used [VsixSynchronizer](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.VsixSynchronizer) to keep code constants in sync with the .VSCT file. This stopped working (automatically) when I converted the project to an SDK style project. Even worse, when I manually invoked it, it overwrote my HybridExtensionPackage.cs file (my VS SDK main entrypoint) instead of HybridExtensionPackage.vsct.cs. I never resolved this problem.
