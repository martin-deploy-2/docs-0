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
+     📁 hello
+       🧾 appsettings.Development.json
+       🧾 appsettings.json
+       🧾 Martin.Hello.csproj
+       🧾 Program.cs
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
      📁 hello
        🧾 appsettings.Development.json
        🧾 appsettings.json
+       🌐 hello.http
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
      📁 hello
+       ✋ .gitignore
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

dotnet publish "./applications/hello" --configuration Release --no-self-contained --output "./applications/hello/publish"

dotnet publish                  -> which, pretty much indicates what we want to do here
"./applications/hello"                  -> the path to the applitation project to build
--configuration Release         -> .NET build configurations bundle compiler options.
                                   The default configuration is 'Debug'; it has compiler options to output debugging information.
                                   The other built-in configuration to 'Release', which has options to request make the compiler optimize your code harder.
--no-self-contained             -> Self-contained applications bundle the .NET runtime along with your own code.
                                   Rather, I'll make a prerequisite for anyone wanting to run my greeter application to install the .NET runtime.
                                   Between the ASP.NET Core Runtime, the .NET Desktop Runtime and then .NET Runtime listed on https://dotnet.microsoft.com/en-us/download/dotnet/7.0,
                                   this will confuse everyone! Just the ASP.NET one has seven options to install it on windows!
                                   Again, we can't have nice things, I'm just doing my part.
--output "./applications/hello/publish" -> The output directory to place the published application in.

save it into a script, call it something relevant, like publish-me-daddy.ps1

```diff
📁 ..
  📁 .
    📁 applications
      📁 hello
        ✋ .gitignore
        🧾 appsettings.Development.json
        🧾 appsettings.json
        🌐 hello.http
        🧾 Martin.Hello.csproj
        🧾 Program.cs
+       🧾 publish-me-daddy.ps1
    📑 README.md
```

publish-me-daddy.ps1

```ps1
dotnet publish "$PSScriptRoot" --configuration Release --no-self-contained --output "$PSScriptRoot/bin/publish"
```

./applications/hello/publish-me-daddy.ps1

```
MSBuild version 17.4.1+9a89d02ff for .NET
  Determining projects to restore...
  All projects are up-to-date for restore.
  Martin.Hello -> ...\docs\applications\hello\bin\Release\net7.0\Martin.Hel
  lo.dll
  Martin.Hello -> ...\docs\applications\hello\bin\publish\
```

ls ./applications/hello/bin/publish

```
📁 ..
  📁 .
    📁 applications
      📁 hello
        📂 bin
          📂 publish
+           🧾 appsettings.Development.json
+           🧾 appsettings.json
+           🧾 Martin.Hello.deps.json
+           📚 Martin.Hello.dll
+           💾 Martin.Hello.exe
+           🧾 Martin.Hello.pdb
+           🧾 Martin.Hello.runtimeconfig.json
+           🧾 web.config
        ✋ .gitignore
        🧾 appsettings.Development.json
        🧾 appsettings.json
        🌐 hello.http
        🧾 Martin.Hello.csproj
        🧾 Program.cs
        🧾 publish-me-daddy.ps1
    📑 README.md
```

admire the published results

the publish folder is full of crap except the `.exe` file, which we can run to execute our app

./applications/hello/bin/publish/Martin.Hello.exe

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: ...\docs
```

it works (of course, it's mine! That feeling wont last for long, though)
c.f. hello.http to try it out

congrat, we now have an app we can run anywhere...
... anywhere there is an ASP.NET runtime, that is, because we can't have nice things.

-->


<!--

oh my godness, containers!


is chose rancher desktop
it has kubernetes integrated
it has nerdctl integrated

if you're disconnected or partially disconnected from the internet, or if Rancher Desktop is misconfigured the slightest, it will fail to start, but, in my case, not before trying out for 10 whole minutes, because we can't have nice things


oc get namespaces

```
NAME              STATUS   AGE
default           Active   13d
kube-system       Active   13d
kube-public       Active   13d
kube-node-lease   Active   13d
```

create a `Containerfile`:

ls

```diff
📁 ..
  📁 .
    📁 applications
      ✋ .gitignore
      🧾 appsettings.Development.json
      🧾 appsettings.json
+     🐳 Containerfile
      🌐 hello.http
      🧾 Martin.Hello.csproj
      🧾 Program.cs
    📑 README.md
```

Containerfile

```Dockerfile
FROM alpine:3.18.0
ENTRYPOINT ["echo", "Hello"]
```

nerdctl image build --tag hello:v0 ./applications/hello

```
[+] Building 8.1s (5/5)
[+] Building 8.2s (5/5) FINISHED
 => [internal] load build definition from Containerfile                                                       0.1s
 => => transferring dockerfile: 90B                                                                           0.0s
 => [internal] load .dockerignore                                                                             0.0s
 => => transferring context: 2B                                                                               0.0s
 => [internal] load metadata for docker.io/library/alpine:3.18.0                                              7.7s
 => CACHED [1/1] FROM docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869  0.0s
 => => resolve docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869a33cb1a  0.0s
 => exporting to docker image format                                                                          0.2s
 => => exporting layers                                                                                       0.0s
 => => exporting manifest sha256:2f4c5d94c32f16477fce285361725612070f8879ea31e8c3cb6633d5beff3e35             0.0s
 => => exporting config sha256:a1cc8d3a3d99163ef35d3e34b90bba7a956146994b9ad052f9b3b5f17fac05f5               0.0s
 => => sending tarball                                                                                        0.1s
Loaded image: docker.io/library/hello:v0
```

nerdctl image list

```
REPOSITORY                                  TAG       IMAGE ID        CREATED          PLATFORM       SIZE         BLOB SIZE
hello                                       v0        2f4c5d94c32f    6 seconds ago    linux/amd64    7.6 MiB      3.2 MiB
```

nerdctl container run --rm hello:v0

```
Hello
```


but of course this isn't satisfying, as we want to put our application in the container, not some random useless bash command


after a bit of search

nerdctl container run --rm -i -t alpine:3.18.0 -- ash
/ # apk

```
apk-tools 2.14.0, compiled for x86_64.

usage: apk [<OPTIONS>...] COMMAND [<ARGUMENTS>...]

Package installation and removal:
  add        Add packages to WORLD and commit changes
  del        Remove packages from WORLD and commit changes

System maintenance:
  fix        Fix, reinstall or upgrade packages without modifying WORLD
  update     Update repository indexes
  upgrade    Install upgrades available from repositories
  cache      Manage the local package cache

Querying package information:
  info       Give detailed information about packages or repositories
  list       List packages matching a pattern or other criteria
  dot        Render dependencies as graphviz graphs
  policy     Show repository policy for packages
  search     Search for packages by name or description

Repository maintenance:
  index      Create repository index file from packages
  fetch      Download packages from repositories to a local directory
  manifest   Show checksums of package contents
  verify     Verify package integrity and signature

Miscellaneous:
  audit      Audit system for changes
  stats      Show statistics about repositories and installations
  version    Compare package versions or perform tests on version strings

This apk has coffee making abilities.
For more information: man 8 apk
```

/ # apk update
fetch https://dl-cdn.alpinelinux.org/alpine/v3.18/main/x86_64/APKINDEX.tar.gz
fetch https://dl-cdn.alpinelinux.org/alpine/v3.18/community/x86_64/APKINDEX.tar.gz
v3.18.3-78-ga74ec4287c7 [https://dl-cdn.alpinelinux.org/alpine/v3.18/main]
v3.18.3-84-gcb6b432d261 [https://dl-cdn.alpinelinux.org/alpine/v3.18/community]
OK: 20071 distinct packages available

/ # apk search dotnet | grep runtime
```
dotnet6-runtime-6.0.21-r0
dotnet7-runtime-7.0.10-r0
```

/ # apk search asp | grep runtime
```
aspnetcore6-runtime-6.0.21-r0
aspnetcore7-runtime-7.0.10-r0
```

we find out that we need the `aspnetcore7-runtime-7.0.10-r0` package

Containerfile

```Dockerfile
FROM alpine:3.18.0
RUN apk add aspnetcore7-runtime-7.0.10-r0
ENTRYPOINT ["dotnet"]
```

nerdctl image build --tag hello:v0 ./applications/hello

```diff
  [+] Building 10.4s (5/5) FINISHED
  => [internal] load .dockerignore                                                                             0.0s
  => => transferring context: 2B                                                                               0.0s
  => [internal] load build definition from Containerfile                                                       0.1s
  => => transferring dockerfile: 126B                                                                          0.0s
  => [internal] load metadata for docker.io/library/alpine:3.18.0                                              7.6s
  => CACHED [1/2] FROM docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869  0.0s
  => => resolve docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869a33cb1a  0.0s
  => ERROR [2/2] RUN apk add aspnetcore7-runtime-7.0.10-r0                                                     2.6s
  ------
  > [2/2] RUN apk add aspnetcore7-runtime-7.0.10-r0:
  #0 0.180 fetch https://dl-cdn.alpinelinux.org/alpine/v3.18/main/x86_64/APKINDEX.tar.gz
  #0 1.474 fetch https://dl-cdn.alpinelinux.org/alpine/v3.18/community/x86_64/APKINDEX.tar.gz
- #0 2.518 ERROR: unable to select packages:
- #0 2.518   aspnetcore7-runtime-7.0.10-r0 (no such package):
- #0 2.518     required by: world[aspnetcore7-runtime-7.0.10-r0]
  ------
  Containerfile:2
  --------------------
  1 |     FROM alpine:3.18.0
  2 | >>> RUN apk add aspnetcore7-runtime-7.0.10-r0
  3 |     ENTRYPOINT ["dotnet"]
  4 |
  --------------------
  error: failed to solve: process "/bin/sh -c apk add aspnetcore7-runtime-7.0.10-r0" did not complete successfully: exit code: 1
  FATA[0010] no image was built
```

`ERROR: unable to select packages: aspnetcore7-runtime-7.0.10-r0 (no such package)` just leaves us alone to figure out that `-7.0.10-r0` is the package version,and it shouldn't be included in the `apk add` parameters


Containerfile

```Dockerfile
FROM alpine:3.18.0
RUN apk add aspnetcore7-runtime
ENTRYPOINT ["dotnet"]
```

nerdctl image build --tag hello:v0 ./applications/hello

```
[+] Building 19.9s (5/6)
[+] Building 20.1s (6/6) FINISHED
 => [internal] load .dockerignore                                                                             0.0s
 => => transferring context: 2B                                                                               0.0s
 => [internal] load build definition from Containerfile                                                       0.1s
 => => transferring dockerfile: 116B                                                                          0.0s
 => [internal] load metadata for docker.io/library/alpine:3.18.0                                              7.6s
 => CACHED [1/2] FROM docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869  0.0s
 => => resolve docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869a33cb1a  0.0s
 => [2/2] RUN apk add aspnetcore7-runtime                                                                     5.8s
 => exporting to docker image format                                                                          6.4s
 => => exporting layers                                                                                       5.1s
 => => exporting manifest sha256:af6c53232709ac3f3c4a1eccfd2ecdd2fe3f70dd58c18ef862fd715dd562c40b             0.0s
 => => exporting config sha256:f06725ec791ec392503e9ebd61011f73b40097b9d9524465c42a6592b71206e3               0.0s
 => => sending tarball                                                                                        1.3s
Loaded image: docker.io/library/hello:v0
```

nerdctl container run --rm hello:v0
```
Usage: dotnet [options]
Usage: dotnet [path-to-application]

Options:
  -h|--help         Display help.
  --info            Display .NET information.
  --list-sdks       Display the installed SDKs.
  --list-runtimes   Display the installed runtimes.

path-to-application:
  The path to an application .dll file to execute.
```

successfully invoking the dotnet command

this is dotnet, but still not our application
in order to get there, we ought to teleport our published application excutable and company into the container

Containerfile

```Dockerfile
FROM alpine:3.18.0
RUN apk add aspnetcore7-runtime
COPY ./bin/publish /opt/hello
ENTRYPOINT ["/opt/hello/Martin.Hello.exe"]
```

nerdctl image build --tag hello:v0 ./applications/hello

```
[+] Building 7.1s (7/8)
[+] Building 7.2s (8/8)
[+] Building 7.3s (8/8) FINISHED
 => [internal] load .dockerignore                                                                             0.1s
 => => transferring context: 2B                                                                               0.1s
 => [internal] load build definition from Containerfile                                                       0.1s
 => => transferring dockerfile: 170B                                                                          0.1s
 => [internal] load metadata for docker.io/library/alpine:3.18.0                                              5.2s
 => [1/3] FROM docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869a33cb1a  0.0s
 => => resolve docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869a33cb1a  0.0s
 => [internal] load build context                                                                             0.2s
 => => transferring context: 182.62kB                                                                         0.2s
 => CACHED [2/3] RUN apk add aspnetcore7-runtime                                                              0.0s
 => [3/3] COPY ./bin/publish /opt/hello                                                                       0.0s
 => exporting to docker image format                                                                          1.5s
 => => exporting layers                                                                                       0.1s
 => => exporting manifest sha256:8656258f53c817950c7a7a570e1f7b84cd2bbfa8fac33857932e291f7efaa2be             0.0s
 => => exporting config sha256:871d784b1e4b01f9854f9114ab93a20fa87c74a40f27b4e57ce0000a4524e2b3               0.0s
 => => sending tarball
```

nerdctl container run --rm hello:v0

```
<3>WSL (1) ERROR: ConfigInitializeEntry:1554: Failed to mount (null) at /dev as devtmpfs 1
```

this is because we have a windows executable, and our container runs Alpine (Linux).
instead, we need to use the DLL, and call the dotnet command on it

```diff
  FROM alpine:3.18.0
  RUN apk add aspnetcore7-runtime
  COPY ./bin/publish /opt/hello
- ENTRYPOINT [          "/opt/hello/Martin.Hello.exe"]
+ ENTRYPOINT ["dotnet", "/opt/hello/Martin.Hello.dll"]
```


nerdctl image build --tag hello:v0 ./applications/hello

```
[+] Building 4.5s (8/8)
[+] Building 4.6s (8/8)
[+] Building 4.7s (8/8) FINISHED
 => [internal] load build definition from Containerfile                                                       0.0s
 => => transferring dockerfile: 180B                                                                          0.0s
 => [internal] load .dockerignore                                                                             0.1s
 => => transferring context: 2B                                                                               0.0s
 => [internal] load metadata for docker.io/library/alpine:3.18.0                                              2.5s
 => [1/3] FROM docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869a33cb1a  0.1s
 => => resolve docker.io/library/alpine:3.18.0@sha256:02bb6f428431fbc2809c5d1b41eab5a68350194fb508869a33cb1a  0.0s
 => [internal] load build context                                                                             0.4s
 => => transferring context: 468B                                                                             0.4s
 => CACHED [2/3] RUN apk add aspnetcore7-runtime                                                              0.0s
 => CACHED [3/3] COPY ./bin/publish /opt/hello                                                                0.0s
 => exporting to docker image format                                                                          1.4s
 => => exporting layers                                                                                       0.0s
 => => exporting manifest sha256:87a37b211f80c867c7adeb0a8e862731913c752a40e455319647a9dc44223e5b             0.0s
 => => exporting config sha256:008180fcafadf0fe15745095a4440840ada5a307878434c589d23a206757b445               0.0s
 => => sending tarball
```

nerdctl container run --rm hello:v0

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /
```

However accessing http://localhost:5000 from out hello.http will not work

```
The connection was rejected. Either the requested service isn’t running on the requested server/port, the proxy settings in vscode are misconfigured, or a firewall is blocking requests. Details: RequestError: connect ECONNREFUSED 127.0.0.1:5000.
```

This happens because hitting http://localhost from the host machine will target the host machine itself, whereas we really want to target the http://localhost of the container running on the rancher VM! Here again, we can't have nice things.

still, this pitfall won't stop us, as it can be circumvented by linking ports of the container with ports of the container's host. this process is reffered to as publishing, or forwarding

let's use that technique to our advantage and link the port 5000 on the container to the port 5000 on the host. in which order? I don't know!

nerdctl container run --rm --publish 5000:5000 hello:v0

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /
```

but our HTTP requests still don't go thought

```
read ECONNRESET
```

is it because the applicatin has an issue?

nerdctl container exec -t -i hello-a970f ash
/ # whoami

```
root
```

/ # apk add curl

```
(1/7) Installing ca-certificates (20230506-r0)
(2/7) Installing brotli-libs (1.0.9-r14)
(3/7) Installing libunistring (1.1-r1)
(4/7) Installing libidn2 (2.3.4-r1)
(5/7) Installing nghttp2-libs (1.55.1-r0)
(6/7) Installing libcurl (8.2.1-r0)
(7/7) Installing curl (8.2.1-r0)
Executing busybox-1.36.0-r9.trigger
Executing ca-certificates-20230506-r0.trigger
OK: 139 MiB in 33 packages
```

/ # curl http://localhost:5000/Martin

```
Hello, Martin.
```

could it be because we're not using the `mcr.microsoft.com/dotnet/aspnet:7.0` base image?

Containerfile

```diff
- FROM alpine:3.18.0
+ FROM mcr.microsoft.com/dotnet/aspnet:7.0
- RUN apk add aspnetcore7-runtime
  COPY ./bin/publish /opt/hello
  ENTRYPOINT ["dotnet", "/opt/hello/Martin.Hello.dll"]
```

nerdctl image build --tag hello:v0 ./applications/hello
```
[+] Building 13.2s (7/7)
[+] Building 13.4s (7/7) FINISHED
 => [internal] load build definition from Containerfile                                                       0.1s
 => => transferring dockerfile: 167B                                                                          0.0s
 => [internal] load .dockerignore                                                                             0.1s
 => => transferring context: 2B                                                                               0.0s
 => [internal] load metadata for mcr.microsoft.com/dotnet/aspnet:7.0                                          4.4s
 => [internal] load build context                                                                             0.3s
 => => transferring context: 468B                                                                             0.3s
 => [1/2] FROM mcr.microsoft.com/dotnet/aspnet:7.0@sha256:54a3864f1c7dbb232982f61105aa18a59b976382a4e720fe18  5.8s
 => => resolve mcr.microsoft.com/dotnet/aspnet:7.0@sha256:54a3864f1c7dbb232982f61105aa18a59b976382a4e720fe18  0.0s
 => => sha256:31e2a952779c41cf5cd187ae1ae276f3de3965a43d41a0250b56c58e1206db8a 10.12MB / 10.12MB              0.6s
 => => sha256:cbca4ad4689cc576651411faa2b98dff307dc8f6de9adf08a5a2d6ae2239d233 154B / 154B                    0.6s
 => => sha256:6c3981608c2b6b3825fe63b9c920101d7bf4ca80ceda7ddc0dc09fdc93a5797d 14.97MB / 14.97MB              1.1s
 => => sha256:a2e354eccee4ad7336b4692c2662de61a68677cec551530c6d7045bcbb3cd6d7 32.45MB / 32.45MB              2.1s
 => => sha256:14726c8f78342865030f97a8d3492e2d1a68fbd22778f9a31dc6be4b4f12a9bc 31.42MB / 31.42MB              2.5s
 => => extracting sha256:14726c8f78342865030f97a8d3492e2d1a68fbd22778f9a31dc6be4b4f12a9bc                     1.1s
 => => extracting sha256:6c3981608c2b6b3825fe63b9c920101d7bf4ca80ceda7ddc0dc09fdc93a5797d                     0.4s
 => => extracting sha256:a2e354eccee4ad7336b4692c2662de61a68677cec551530c6d7045bcbb3cd6d7                     0.8s
 => => extracting sha256:cbca4ad4689cc576651411faa2b98dff307dc8f6de9adf08a5a2d6ae2239d233                     0.0s
 => => extracting sha256:31e2a952779c41cf5cd187ae1ae276f3de3965a43d41a0250b56c58e1206db8a                     0.3s
 => [2/2] COPY ./bin/publish /opt/hello                                                                       0.2s
 => exporting to docker image format                                                                          2.7s
 => => exporting layers                                                                                       0.2s
 => => exporting manifest sha256:e9b1bee8d602a2e4b21fe3e31c0e155693011a6faa3ed546d44662ad1b352088             0.0s
 => => exporting config sha256:f2e6f1b45369056f30266289768dbd79c34898a54a94c3ba952e419cb51e5ba0               0.0s
 => => sending tarball                                                                                        2.4s
Loaded image: docker.io/library/hello:v0
```

nerdctl container run --rm --publish 5000:5000 hello:v0

```
  info: Microsoft.Hosting.Lifetime[14]
+       Now listening on: http://[::]:80
  info: Microsoft.Hosting.Lifetime[0]
        Application started. Press Ctrl+C to shut down.
  info: Microsoft.Hosting.Lifetime[0]
        Hosting environment: Production
  info: Microsoft.Hosting.Lifetime[0]
        Content root path: /
```

but there is a problem, as our app now listens on port 80 instead of 5000
we have to link port 80

nerdctl container run --rm --publish 80:80 hello:v0

http://localhost:80 ?

```
HTTP/1.1 404 Not Found
Content-Type: text/plain; charset=utf-8
X-Content-Type-Options: nosniff
Date: Fri, 25 Aug 2023 13:22:27 GMT
Content-Length: 19
Connection: close

404 page not found
```

As silly as it seems, let's test the possibility that linking a port number on the container to the same port number on the host is the origin of the problem.

link the container port 80 to host port 5000

nerdctl container run --rm --publish 80:5000 hello:v0

http://localhost:5000 ?

```
The connection was rejected. Either the requested service isn’t running on the requested server/port, the proxy settings in vscode are misconfigured, or a firewall is blocking requests. Details: RequestError: connect ECONNREFUSED 127.0.0.1:5000.
```

is it the other way around, then?

nerdctl container run --rm --publish 5000:80 hello:v0

http://localhost:5000 ?

```
HTTP/1.1 200 OK
Connection: close
Content-Type: text/plain; charset=utf-8
Date: Fri, 25 Aug 2023 13:25:57 GMT
Server: Kestrel
Transfer-Encoding: chunked

Hello.
```

Of course it is! Because we can't have nice things!


Let's get back to using `alpine:3.18.0` as our base image and installing `aspnetcore7-runtime`, then run the container linking ports differently:

nerdctl container run --rm --publish 8080:5000 hello:v0
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /
```

http://localhost:80

```
read ECONNRESET
```

It looks like we are stuck with our `mcr.microsoft.com/dotnet/aspnet:7.0`-branded leash. because we can't have nice things.

-->

