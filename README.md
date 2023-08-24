# GitOps - My Journey from Zero to Negative One

<!--


we can't have nive things


At the dawn of time, when times were harsh,
an APPLICATION
    runs on a MACHINE
=> "It works on my machine"

then, after decoupling the application from the machine,
an APPLICATION
	runs on an OS
		that operates a MACHINE
=> "It works on my OS"

because we still thought the application was too close from the machine (and because applications started having expectations about the underlying OS)
an APPLICATION
	runs on GUEST OS
		in a VM
			on a HOST OS
				that operates the MACHINE
=> "It works in my VM"

when virtual machines got too slow for our speed greed,
an APPLICATION
	runs on HALF AN OS
		isolated in a CONTAINER
			sharing its other half with a HOST OS
				that operates the MACHINE
=> "It works in my container"

but, thanks the gods, today,
an APPLICATION
	runs on HALF AN OS
		isolated in its CONTAINER
			wrapped by a POD
				part of a REPLICA SET
					managed by a DEPLOYMENT
						running on KUBERNETES-FIRST OS
							in a VM (you really didn't think they'd go away so easily)
								forming a CLUSTER
									provisioned in the CLOUD
=> "It works on my cluster"

Application release management has never been so streamlined.

-->


<!--


at the essence of the universe is the application


create a web app

ls

```
📁 ..
  📁 .
    📑 README.md
```

make it simple
start with a greeter
use .NET (hey, nobody is perfect)
call it hello

dotnet new create "web" --name "Martin.Hello" --exclude-launch-settings --output ./applications/hello

```
The template "ASP.NET Core Empty" was created successfully.

Processing post-creation actions...
Restoring ...\applications\hello\Martin.Hello.csproj:
  Determining projects to restore...
  Restored ...\applications\hello\Martin.Hello.csproj (in 87 ms).
Restore succeeded.
```

ls

```diff
📁 ..
  📁 .
+   📁 applications
+     🧾 appsettings.Development.json
+     🧾 appsettings.json
+     🧾 Martin.Hello.csproj
+     🧾 Program.cs
    📑 README.md
```

Program.cs

```cs
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "Hello World!");

        app.Run();
```

dotnet run --project ./applications/hello

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: ...\applications\hello
```

of course if we navigate to http://localhost:5000, it will show

```
Hello World!
```

But I don't want to open and fiddle around with my browser all the time, because I'm lazy, so I'll create a `hello.http` file and use [REST Client for Visual Studio Code](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) instead.

ls

```diff
📁 ..
  📁 .
    📁 applications
      🧾 appsettings.Development.json
      🧾 appsettings.json
+     🌐 hello.http
      🧾 Martin.Hello.csproj
      🧾 Program.cs
    📑 README.md
```

hello.http

```http
@BASE_URL = "http://localhost:5000"

GET {{BASE_URL}}
```

Trying that out will slap you back in the face with an unkind error message:

> The connection was rejected. Either the requested service isn’t running on the requested server/port, the proxy settings in vscode are misconfigured, or a firewall is blocking requests. Details: RequestError: connect ECONNREFUSED 127.0.0.1:443.

The silly solution here being to remove the quotes around the value of `@BASE_URL = "http://localhost:5000"`, because we can't have nice things.

hello.http

```diff
- @BASE_URL = "http://localhost:5000"
+ @BASE_URL =  http://localhost:5000
```

define its behavior
    GET /       => "Hello.", plain text
    GET /<name> => "Hello, <name>."
simple web api

hello.http

```http
@BASE_URL = http://localhost:5000

###############################################################################
# This should return "Hello.", plain-text.

GET {{BASE_URL}}/

###############################################################################
# @prompt NAME
# This should return "Hello, <NAME>.", plain-text.

GET {{BASE_URL}}/{{NAME}}
```


Program.cs

```cs
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

/* - */ app.MapGet("/", () => "Hello World!");

/* + */ app.MapGet("/", () => "Hello.");
/* + */ app.MapGet("/{name}", (string name) => $"Hello, {name}.");

        app.Run();
```

Restarting the application while developing is becoming quite tedious already, so I should use `dotnet watch run` instead of `dotnet run`.

Now http://localhost:5000/Martin returns

```
Hello, Martin.
```

if you're committing to Git, you'll notice the `bin` and `obj` folders are massively full of compiler-generated 💩. I'll make myself a `.gitignore` to save my bandwidth.

ls

```diff
📁 ..
  📁 .
    📁 applications
+     ✋ .gitignore
      🧾 appsettings.Development.json
      🧾 appsettings.json
      🌐 hello.http
      🧾 Martin.Hello.csproj
      🧾 Program.cs
    📑 README.md
```

.gitignore

```
bin/
obj/
```

now that we've got our application, we must build it, and ship it.
easy just press F5, right?
well, no, we need to properly build it, package it, and deploy it.
but there is a problem: I never learned how to do that in Uni, I've always pressed F5!
because, remember: we can't have nice things (some malevolent among you will have said that a first time when I mentioned the use of .NET)

dotnet publish "/tmp/builder" --configuration Release --no-self-contained --output "/tmp/builder/publish"

dotnet publish                  -> which, pretty much indicates what we want to do here
"/tmp/builder"                  -> the path to the applitation project to build
--configuration Release         -> .NET build configurations bundle compiler options.
                                   The default configuration is 'Debug'; it has compiler options to output debugging information.
                                   The other built-in configuration to 'Release', which has options to request make the compiler optimize your code harder.
--no-self-contained             -> Self-contained applications bundle the .NET runtime along with your own code.
                                   Rather, I'll make a prerequisite for anyone wanting to run my greeter application to install the .NET runtime.
                                   Between the ASP.NET Core Runtime, the .NET Desktop Runtime and then .NET Runtime listed on https://dotnet.microsoft.com/en-us/download/dotnet/7.0,
                                   this will confuse everyone! Just the ASP.NET one has seven options to install it on windows!
                                   Again, we can't have nice things, I'm just doing my part.
--output "/tmp/builder/publish" -> The output directory to place the published application in.

save it into a script, call it publish-me-daddy.ps1

admire the published results
run te published app
it works (of course, it's mine! That feeling wont last for long, though)

-->


<!--

oh my godness, containers!

-->


