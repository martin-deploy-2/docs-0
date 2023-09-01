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
  Martin.Hello -> ...\docs\applications\hello\bin\Release\net7.0\Martin.Hello.dll
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
      📁 hello
        ✋ .gitignore
        🧾 appsettings.Development.json
        🧾 appsettings.json
+       🐳 Containerfile
        🌐 hello.http
        🧾 Martin.Hello.csproj
        🧾 Program.cs
        🧾 publish-me-daddy.ps1
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

<!--

Pod

the pod groups containers
it provides shared storage, in the form of volumes, which can also point to external sources
all containers in a pod share an IP address, ports, and `localhost`
all containers in a pod are scheduled on the same node

YAML is not a markup language
it is a data serialization language
as such it can be used to serialize data, but it's not very good at it
or as a markup language, but it's not very good at it
or to declare configuration, but it's not very good at it either

the configuration of a pod can be declared in YAML

create a yaml file to define a pod:

ls

```diff
📁 ..
  📁 .
    📁 applications
      📁 hello
        ✋ .gitignore
        🧾 appsettings.Development.json
        🧾 appsettings.json
        🐳 Containerfile
        🌐 hello.http
        🧾 Martin.Hello.csproj
        🧾 Program.cs
        🧾 publish-me-daddy.ps1
    📁 pods
+     🧾 hello.yaml
    📑 README.md
```

hello.yaml:

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: hello
spec:
  containers:
    - name: app
      image: alpine:3.18.0
      command: [echo]
      args: ["Hello."]
```

kubectl apply --filename ./pods/hello.yaml
```
pod/hello created
```

see if we can list it

kubectl get pods
```
NAME    READY   STATUS      RESTARTS      AGE
hello   0/1     Completed   2 (21s ago)   23s
```

of course our pod is marked as Completed and must have died along the way, as proven by the same command

kubectl get pods
```
NAME    READY   STATUS             RESTARTS      AGE
hello   0/1     CrashLoopBackOff   3 (35s ago)   80s
```

because kubernetes is a mad world where you can't run the same command over and over and expect the same result
still, in our first exposure case, it's not quite as critical as it seems, as the container simply did its job of echoing "Hello."

kubectl logs pod/hello
```
Hello.
```

to prevent the container from dying, we can make a loop:

hello.yaml

```diff
  apiVersion: v1
  kind: Pod
  metadata:
    name: hello
  spec:
    containers:
      - name: app
        image: alpine:3.18.0
-       command: [echo]
+       command: [ash, -c]
-       args: ["Hello."]
+       args: ['while true; do echo "Hello."; sleep 1; done']
```

kubectl apply --filename ./pods/hello.yaml
```
The Pod "hello" is invalid: spec: Forbidden: pod updates may not change fields other than `spec.containers[*].image`, `spec.initContainers[*].image`, `spec.activeDeadlineSeconds`, `spec.tolerations` (only additions to existing tolerations) or `spec.terminationGracePeriodSeconds` (allow it to be set to 1 if it was previously negative)
  core.PodSpec{
        Volumes:        {{Name: "kube-api-access-njbtv", VolumeSource: {Projected: &{Sources: {{ServiceAccountToken: &{ExpirationSeconds: 3607, Path: "token"}}, {ConfigMap: &{LocalObjectReference: {Name: "kube-root-ca.crt"}, Items: {{Key: "ca.crt", Path: "ca.crt"}}}}, {DownwardAPI: &{Items: {{Path: "namespace", FieldRef: &{APIVersion: "v1", FieldPath: "metadata.namespace"}}}}}}, DefaultMode: &420}}}},
        InitContainers: nil,
        Containers: []core.Container{
                {
                        Name:       "app",
                        Image:      "alpine:3.18.0",
                        Command:    {"ash", "-c"},
-                       Args:       []string{"echo Hello."},
+                       Args:       []string{`while true; do echo "Hello."; sleep 1; done`},
                        WorkingDir: "",
                        Ports:      nil,
                        ... // 16 identical fields
                },
        },
        EphemeralContainers: nil,
        RestartPolicy:       "Always",
        ... // 26 identical fields
  }
```

I forgot to mention that a pod is mostly immutable, the only mutable fields being of low interest, and the container image being a case were we generally prefer a full pod restart.

to circumvent this, we can either murder the pod using `kubectl delete pod hello`, or use force:

kubectl apply --filename ./pods/hello.yaml --force
```
pod/hello configured
```


kubectl get pods
```
NAME    READY   STATUS    RESTARTS   AGE
hello   1/1     Running   0          54s
```

kubectl logs pod/hello --follow
```
Hello.
Hello.
Hello.
Hello.
Hello.
Hello.
Hello.
Hello.
Hello.
Hello.
```

and will print more `Hello.` until you press CTRL+C or murder the pod.
sweet, I hear you thinking, let's deploy all our applications in containers within a pod!
Let's take the example of two containers, happily running together in the same pod
one container is quite stable, but the other one is prone to terminating on the error side of the exit code
to simulate this, we'll add another container to our pod and make it error out after 10 seconds

hello.yaml

```diff
  apiVersion: v1
  kind: Pod
  metadata:
    name: hello
  spec:
    containers:
      - name: app
        image: alpine:3.18.0
        command: [ash, -c]
-       args: ['while true; do echo "Hello."; sleep 1; done']
+       args: ['DATE=$(date); while true; do echo "$DATE - Hello."; sleep 1; done']
+     - name: el-contenedito-de-la-muerte
+       image: alpine:3.18.0
+       command: [ash, -c]
+       args: ['for i in $(seq 1 10); do echo "Hello, $i."; sleep 1; done; exit 666']
```

kubectl apply --filename ./pods/hello.yaml --force

```
pod/hello configured
```


`kubectl logs pod/hello --container app --follow` and wait 10s

```
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
Wed Aug 30 15:30:05 UTC 2023 - Hello.
```

One failing container will not sink the whole pod, but will put it in a CrashLoopBackOff state:

kubectl get pods
```
NAME    READY   STATUS             RESTARTS      AGE
hello   1/2     CrashLoopBackOff   5 (95s ago)   5m35s
```

however, we will be facing another issue, because we can't have nice things, remember?
all containers in a pod are scheduled on the same node, which, on a single-node cluster like a local Rancher Desktop development environment, is not problematic, but it will become bad on a fully fledged cloud-provisioned cluster with dozens of nodes. And that's without talking about scaling and replication, the needs for which will vary per application (container), while the smallest building block of Kubernetes is the pod.

let's deploy our hello application, replacing the image by `hello:v0`, and trust our Containerfile's ENTRYPOINT to call .NET

hello.yaml
```diff
  apiVersion: v1
  kind: Pod
  metadata:
    name: hello
  spec:
    containers:
      - name: app
-       image: alpine:3.18.0
+       image: hello:v0
-       command: [ash, -c]
-       args: ['DATE=$(date); while true; do echo "$DATE - Hello."; sleep 1; done']
-     - name: el-contenedito-de-la-muerte
-       image: alpine:3.18.0
-       command: [ash, -c]
-       args: ['for i in $(seq 1 10); do echo "Hello, $i."; sleep 1; done; exit 666']
```


kubectl apply --filename ./pods/hello.yaml --force
```
pod/hello configured
```

kubectl get pods
```
NAME    READY   STATUS         RESTARTS   AGE
hello   0/1     ErrImagePull   0          15s
```

kubectl describe pod/hello
```
Name:             hello
Namespace:        default
Service Account:  default
Node:             .../...
Start Time:       Wed, 30 Aug 2023 16:43:43 +0100
Labels:           <none>
Status:           Pending
IP:               10.42.0.140
IPs:
  IP:  10.42.0.140
Containers:
  app:
    Container ID:
    Image:          hello:v0
    Image ID:
    Port:           <none>
    Host Port:      <none>
    State:          Waiting
      Reason:       ImagePullBackOff
    Ready:          False
    Restart Count:  0
    Environment:    <none>
    Mounts:
      /var/run/secrets/kubernetes.io/serviceaccount from kube-api-access-zz26d (ro)
Conditions:
  Type              Status
  Initialized       True
  Ready             False
  ContainersReady   False
  PodScheduled      True
Volumes:
  kube-api-access-zz26d:
    Type:                    Projected (a volume that contains injected data from multiple sources)
    TokenExpirationSeconds:  3607
    ConfigMapName:           kube-root-ca.crt
    ConfigMapOptional:       <nil>
    DownwardAPI:             true
QoS Class:                   BestEffort
Node-Selectors:              <none>
Tolerations:                 node.kubernetes.io/not-ready:NoExecute op=Exists for 300s
                             node.kubernetes.io/unreachable:NoExecute op=Exists for 300s
Events:
  Type     Reason     Age                From               Message
  ----     ------     ----               ----               -------
  Normal   Scheduled  34s                default-scheduler  Successfully assigned default/hello to ...
  Normal   BackOff    29s                kubelet            Back-off pulling image "hello:v0"
  Warning  Failed     29s                kubelet            Error: ImagePullBackOff
  Normal   Pulling    15s (x2 over 34s)  kubelet            Pulling image "hello:v0"
  Warning  Failed     10s (x2 over 29s)  kubelet            Failed to pull image "hello:v0": rpc error: code = Unknown desc = failed to pull and unpack image "docker.io/library/hello:v0": failed to resolve reference "docker.io/library/hello:v0": pull access denied, repository does not exist or may require authorization: server message: insufficient_scope: authorization failed
  Warning  Failed     10s (x2 over 29s)  kubelet            Error: ErrImagePull
```

the interesting bit here is the Events section, in which we learn that kubernetes failed to pull and unpack image "docker.io/library/hello:v0"
looks like it queries the docker registry, even if I have the image locally, because we can't heve nice things
to make our images available to the local kubernetess cluster, we have to build them with `nerdctl` in the `k8s.io` "namespace"
nerdctl namespaces are not the same as kubernetes namespaces, because what else

nerdctl image build --tag hello:v0 --namespace k8s.io ./applications/hello
```
[+] Building 4.6s (7/7)
[+] Building 4.6s (7/7) FINISHED
 => [internal] load .dockerignore                                                                             0.0s
 => => transferring context: 2B                                                                               0.0s
 => [internal] load build definition from Containerfile                                                       0.1s
 => => transferring dockerfile: 167B                                                                          0.0s
 => [internal] load metadata for mcr.microsoft.com/dotnet/aspnet:7.0                                          2.2s
 => [internal] load build context                                                                             0.3s
 => => transferring context: 468B                                                                             0.3s
 => [1/2] FROM mcr.microsoft.com/dotnet/aspnet:7.0@sha256:54a3864f1c7dbb232982f61105aa18a59b976382a4e720fe18  0.0s
 => => resolve mcr.microsoft.com/dotnet/aspnet:7.0@sha256:54a3864f1c7dbb232982f61105aa18a59b976382a4e720fe18  0.0s
 => CACHED [2/2] COPY ./bin/publish /opt/hello                                                                0.0s
 => exporting to docker image format                                                                          2.0s
 => => exporting layers                                                                                       0.0s
 => => exporting manifest sha256:e9b1bee8d602a2e4b21fe3e31c0e155693011a6faa3ed546d44662ad1b352088             0.0s
 => => exporting config sha256:f2e6f1b45369056f30266289768dbd79c34898a54a94c3ba952e419cb51e5ba0               0.0s
 => => sending tarball                                                                                        1.9s
Loaded image: docker.io/library/hello:v0
```

nerdctl image list --namespace k8s.io
```
REPOSITORY                                  TAG                     IMAGE ID        CREATED           PLATFORM       SIZE         BLOB SIZE
alpine                                      3.18.0                  02bb6f428431    2 weeks ago       linux/amd64    7.6 MiB      3.2 MiB
hello                                       v0                      e9b1bee8d602    14 seconds ago    linux/amd64    214.8 MiB    84.9 MiB
```

now our application is running fine:

kubectl get pods
```
NAME    READY   STATUS    RESTARTS   AGE
hello   1/1     Running   0          11m
```

kubectl logs pod/hello
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://[::]:80
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /
```

But how do we access it?
The first step would be to ensure the web service is accessible, but CURLing it from within te container
unlike me, you will remember that the microsoft .NET base image uses Debian, as opposed to Alpine,
and you will not loose your precious time trying to understand why `ash` is suddently unavailable


kubectl exec pod/hello -- apt update
```
WARNING: apt does not have a stable CLI interface. Use with caution in scripts.

Get:1 http://deb.debian.org/debian bullseye InRelease [116 kB]
Get:2 http://deb.debian.org/debian-security bullseye-security InRelease [48.4 kB]
Get:3 http://deb.debian.org/debian bullseye-updates InRelease [44.1 kB]
Get:4 http://deb.debian.org/debian bullseye/main amd64 Packages [8183 kB]
Get:5 http://deb.debian.org/debian-security bullseye-security/main amd64 Packages [245 kB]
Get:6 http://deb.debian.org/debian bullseye-updates/main amd64 Packages [17.5 kB]
Fetched 8653 kB in 2s (3701 kB/s)
Reading package lists...
Building dependency tree...
Reading state information...
All packages are up to date.
```

kubectl exec pod/hello -- apt install curl -y

```
WARNING: apt does not have a stable CLI interface. Use with caution in scripts.

Reading package lists...
Building dependency tree...
Reading state information...
The following additional packages will be installed:
  libbrotli1 libcurl4 libldap-2.4-2 libldap-common libnghttp2-14 libpsl5
  librtmp1 libsasl2-2 libsasl2-modules libsasl2-modules-db libssh2-1
  publicsuffix
Suggested packages:
  libsasl2-modules-gssapi-mit | libsasl2-modules-gssapi-heimdal
  libsasl2-modules-ldap libsasl2-modules-otp libsasl2-modules-sql
The following NEW packages will be installed:
  curl libbrotli1 libcurl4 libldap-2.4-2 libldap-common libnghttp2-14 libpsl5
  librtmp1 libsasl2-2 libsasl2-modules libsasl2-modules-db libssh2-1
  publicsuffix
0 upgraded, 13 newly installed, 0 to remove and 0 not upgraded.
Need to get 1981 kB of archives.
After this operation, 4363 kB of additional disk space will be used.
Get:1 http://deb.debian.org/debian bullseye/main amd64 libbrotli1 amd64 1.0.9-2+b2 [279 kB]
Get:2 http://deb.debian.org/debian bullseye/main amd64 libsasl2-modules-db amd64 2.1.27+dfsg-2.1+deb11u1 [69.1 kB]
Get:3 http://deb.debian.org/debian bullseye/main amd64 libsasl2-2 amd64 2.1.27+dfsg-2.1+deb11u1 [106 kB]
Get:4 http://deb.debian.org/debian bullseye/main amd64 libldap-2.4-2 amd64 2.4.57+dfsg-3+deb11u1 [232 kB]
Get:5 http://deb.debian.org/debian bullseye/main amd64 libnghttp2-14 amd64 1.43.0-1 [77.1 kB]
Get:6 http://deb.debian.org/debian bullseye/main amd64 libpsl5 amd64 0.21.0-1.2 [57.3 kB]
Get:7 http://deb.debian.org/debian bullseye/main amd64 librtmp1 amd64 2.4+20151223.gitfa8646d.1-2+b2 [60.8 kB]
Get:8 http://deb.debian.org/debian bullseye/main amd64 libssh2-1 amd64 1.9.0-2 [156 kB]
Get:9 http://deb.debian.org/debian bullseye/main amd64 libcurl4 amd64 7.74.0-1.3+deb11u7 [346 kB]
Get:10 http://deb.debian.org/debian bullseye/main amd64 curl amd64 7.74.0-1.3+deb11u7 [270 kB]
Get:11 http://deb.debian.org/debian bullseye/main amd64 libldap-common all 2.4.57+dfsg-3+deb11u1 [95.8 kB]
Get:12 http://deb.debian.org/debian bullseye/main amd64 libsasl2-modules amd64 2.1.27+dfsg-2.1+deb11u1 [104 kB]
Get:13 http://deb.debian.org/debian bullseye/main amd64 publicsuffix all 20220811.1734-0+deb11u1 [127 kB]
debconf: delaying package configuration, since apt-utils is not installed
Fetched 1981 kB in 1s (1674 kB/s)
Selecting previously unselected package libbrotli1:amd64.
(Reading database ... 6988 files and directories currently installed.)
Preparing to unpack .../00-libbrotli1_1.0.9-2+b2_amd64.deb ...
Unpacking libbrotli1:amd64 (1.0.9-2+b2) ...
Selecting previously unselected package libsasl2-modules-db:amd64.
Preparing to unpack .../01-libsasl2-modules-db_2.1.27+dfsg-2.1+deb11u1_amd64.deb ...
Unpacking libsasl2-modules-db:amd64 (2.1.27+dfsg-2.1+deb11u1) ...
Selecting previously unselected package libsasl2-2:amd64.
Preparing to unpack .../02-libsasl2-2_2.1.27+dfsg-2.1+deb11u1_amd64.deb ...
Unpacking libsasl2-2:amd64 (2.1.27+dfsg-2.1+deb11u1) ...
Selecting previously unselected package libldap-2.4-2:amd64.
Preparing to unpack .../03-libldap-2.4-2_2.4.57+dfsg-3+deb11u1_amd64.deb ...
Unpacking libldap-2.4-2:amd64 (2.4.57+dfsg-3+deb11u1) ...
Selecting previously unselected package libnghttp2-14:amd64.
Preparing to unpack .../04-libnghttp2-14_1.43.0-1_amd64.deb ...
Unpacking libnghttp2-14:amd64 (1.43.0-1) ...
Selecting previously unselected package libpsl5:amd64.
Preparing to unpack .../05-libpsl5_0.21.0-1.2_amd64.deb ...
Unpacking libpsl5:amd64 (0.21.0-1.2) ...
Selecting previously unselected package librtmp1:amd64.
Preparing to unpack .../06-librtmp1_2.4+20151223.gitfa8646d.1-2+b2_amd64.deb ...
Unpacking librtmp1:amd64 (2.4+20151223.gitfa8646d.1-2+b2) ...
Selecting previously unselected package libssh2-1:amd64.
Preparing to unpack .../07-libssh2-1_1.9.0-2_amd64.deb ...
Unpacking libssh2-1:amd64 (1.9.0-2) ...
Selecting previously unselected package libcurl4:amd64.
Preparing to unpack .../08-libcurl4_7.74.0-1.3+deb11u7_amd64.deb ...
Unpacking libcurl4:amd64 (7.74.0-1.3+deb11u7) ...
Selecting previously unselected package curl.
Preparing to unpack .../09-curl_7.74.0-1.3+deb11u7_amd64.deb ...
Unpacking curl (7.74.0-1.3+deb11u7) ...
Selecting previously unselected package libldap-common.
Preparing to unpack .../10-libldap-common_2.4.57+dfsg-3+deb11u1_all.deb ...
Unpacking libldap-common (2.4.57+dfsg-3+deb11u1) ...
Selecting previously unselected package libsasl2-modules:amd64.
Preparing to unpack .../11-libsasl2-modules_2.1.27+dfsg-2.1+deb11u1_amd64.deb ...
Unpacking libsasl2-modules:amd64 (2.1.27+dfsg-2.1+deb11u1) ...
Selecting previously unselected package publicsuffix.
Preparing to unpack .../12-publicsuffix_20220811.1734-0+deb11u1_all.deb ...
Unpacking publicsuffix (20220811.1734-0+deb11u1) ...
Setting up libpsl5:amd64 (0.21.0-1.2) ...
Setting up libbrotli1:amd64 (1.0.9-2+b2) ...
Setting up libsasl2-modules:amd64 (2.1.27+dfsg-2.1+deb11u1) ...
Setting up libnghttp2-14:amd64 (1.43.0-1) ...
Setting up libldap-common (2.4.57+dfsg-3+deb11u1) ...
Setting up libsasl2-modules-db:amd64 (2.1.27+dfsg-2.1+deb11u1) ...
Setting up librtmp1:amd64 (2.4+20151223.gitfa8646d.1-2+b2) ...
Setting up libsasl2-2:amd64 (2.1.27+dfsg-2.1+deb11u1) ...
Setting up libssh2-1:amd64 (1.9.0-2) ...
Setting up publicsuffix (20220811.1734-0+deb11u1) ...
Setting up libldap-2.4-2:amd64 (2.4.57+dfsg-3+deb11u1) ...
Setting up libcurl4:amd64 (7.74.0-1.3+deb11u7) ...
Setting up curl (7.74.0-1.3+deb11u7) ...
Processing triggers for libc-bin (2.31-13+deb11u6) ...
```

kubectl exec pod/hello -- curl --silent http://localhost:80
```
Hello.
```

-->

<!--

Self Service

network ports listened to by the applications living inside containers wrapped in pods don't get automatically exposed to the outside world
that would be a nice thing, and we can't have those
exposing applications to the network is done by declaring a Service

as over-engineered as it sounds to have another abstraction layer to route network trafic to our pods
services are, at the end of times, not that unlikeable
See, the pods are mostly immutable, and it's the doxa to consider them as disposable at will
it means that those poor things will be sacrificed at any occasion, but they don't mind
[portal they don't feel pain, just a simulation]
for us however this means that we can't rely on the pod's IP address for a long term relationship
not only the services hide the pods, they also do load-balancing, and they trigger the creation of a DNS entry internal to the cluster, which has the form `<SERVICE_NAME>.<SERVICE_NAMESPACE="default">.svc.cluster.local`

let's create a service

```diff
📁 ..
  📁 .
    📁 applications
      📁 hello
        ✋ .gitignore
        🧾 appsettings.Development.json
        🧾 appsettings.json
        🐳 Containerfile
        🌐 hello.http
        🧾 Martin.Hello.csproj
        🧾 Program.cs
        🧾 publish-me-daddy.ps1
    📁 pods
!     🧾 hello.yaml
+   📁 services
+     🧾 hello.yaml
    📑 README.md
```

pods/hello.yaml:

```diff
  apiVersion: v1
  kind: Pod
  metadata:
    name: hello
+   labels:
+     app.kubernetes.io/name: 2545bb8b-e59c-4f4a-b362-2d1591215f25
  spec:
    containers:
      - name: app
        image: hello:v0
```

services/hello.yaml:

```yaml
apiVersion: v1
kind: Service
metadata:
  name: hello
spec:
  selector:
    app.kubernetes.io/name: 2545bb8b-e59c-4f4a-b362-2d1591215f25
  ports:
    - port: 5000
      targetPort: 80
```

both the pod and the service have their `.metadata.name` set to `hello`
but that doesn't matter, because they are resources of different types

a service knows what pods to pass trafic to, not by the pods' names, which might change as much as pods are created and ditched, but by matching their "labels"

labels are like tags that take the form of a string-string key-value pairs

Kubernetes defines a bunch of [standard labels](https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/#labels), among which `app.kubernetes.io/name`, which defines the name of the considered application
which sounds like a safe starter for identifying pod wrapping containers in which applications live
for the value, I'll use the second worst thing after a poor name: a giberrish GUID!

services allow for changing the exposed network port
the `.spec.ports[].port` is the port exposed by the service
while the `.spec.ports[].targetPort` is the port the application in a container in a pod is listening to

kubectl apply --filename ./pods/hello.yaml --force --filename ./services/hello.yaml
```
pod/hello configured
service/hello created
```

kubectl get pods,services
```
NAME        READY   STATUS    RESTARTS   AGE
pod/hello   1/1     Running   0          4m53s

NAME                 TYPE        CLUSTER-IP     EXTERNAL-IP   PORT(S)    AGE
service/kubernetes   ClusterIP   10.43.0.1      <none>        443/TCP    21d
service/hello        ClusterIP   10.43.46.187   <none>        5000/TCP   5m11s
```

Spawn another pod to CURL the application using our new service.

kubectl run (new-guid) --image alpine:3.18.0 --rm --tty --stdin -- ash
```
If you don't see a command prompt, try pressing enter.
```

/ # apk add curl
```
fetch https://dl-cdn.alpinelinux.org/alpine/v3.18/main/x86_64/APKINDEX.tar.gz
fetch https://dl-cdn.alpinelinux.org/alpine/v3.18/community/x86_64/APKINDEX.tar.gz
(1/7) Installing ca-certificates (20230506-r0)
(2/7) Installing brotli-libs (1.0.9-r14)
(3/7) Installing libunistring (1.1-r1)
(4/7) Installing libidn2 (2.3.4-r1)
(5/7) Installing nghttp2-libs (1.55.1-r0)
(6/7) Installing libcurl (8.2.1-r0)
(7/7) Installing curl (8.2.1-r0)
Executing busybox-1.36.0-r9.trigger
Executing ca-certificates-20230506-r0.trigger
OK: 12 MiB in 22 packages
```

/ # curl http://hello.default.svc.cluster.local:5000
```
Hello.
```

in the rancher desktop UI, nav to the Port Forwarding tab, find the hello service, click the Forward button, then use 5000 as the Local Port value, click the check mark

Use `hello.http` to GET http://localhost:5000:

```
Hello.
```

-->


<!--

Replica Sets

update the application so that it displays a unique id that is set only once at startup of the application

```diff
📁 ..
  📁 .
    📁 applications
      📁 hello
        ✋ .gitignore
        🧾 appsettings.Development.json
        🧾 appsettings.json
        🐳 Containerfile
        🌐 hello.http
        🧾 Martin.Hello.csproj
!       🧾 Program.cs
        🧾 publish-me-daddy.ps1
    📁 pods
      🧾 hello.yaml
    📁 services
      🧾 hello.yaml
    📑 README.md
```

Program.cs:

```diff
  var builder = WebApplication.CreateBuilder(args);
  var app = builder.Build();
+ var aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication = Guid.NewGuid().ToString();

- app.MapGet("/", () =>                                                        $"Hello.");
+ app.MapGet("/", () => $"{aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication}\nHello.");
- app.MapGet("/{name}", (string name) =>                                                        $"Hello, {name}.");
+ app.MapGet("/{name}", (string name) => $"{aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication}\nHello, {name}.");

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

GET http://localhost:5000
```
5a854efa-5441-4efe-9b53-f66ec409822a
Hello.
```

Not only that, but you will always get the same id until the application restarts

./applications/hello/publish-me-daddy.ps1
```
MSBuild version 17.4.1+9a89d02ff for .NET
  Determining projects to restore...
  All projects are up-to-date for restore.
  Martin.Hello -> ...\applications\hello\bin\Release\net7.0\Martin.Hello.dll
  Martin.Hello -> ...\applications\hello\bin\publish\
```

nerdctl image build --tag hello:v1 --namespace k8s.io ./applications/hello
```
[+] Building 4.6s (6/7)
[+] Building 4.7s (7/7)
[+] Building 4.8s (7/7) FINISHED
 => [internal] load build definition from Containerfile                                                       0.0s
 => => transferring dockerfile: 167B                                                                          0.0s
 => [internal] load .dockerignore                                                                             0.0s
 => => transferring context: 2B                                                                               0.0s
 => [internal] load metadata for mcr.microsoft.com/dotnet/aspnet:7.0                                          2.2s
 => [internal] load build context                                                                             0.3s
 => => transferring context: 181.41kB                                                                         0.3s
 => CACHED [1/2] FROM mcr.microsoft.com/dotnet/aspnet:7.0@sha256:54a3864f1c7dbb232982f61105aa18a59b976382a4e  0.0s
 => => resolve mcr.microsoft.com/dotnet/aspnet:7.0@sha256:54a3864f1c7dbb232982f61105aa18a59b976382a4e720fe18  0.0s
 => [2/2] COPY ./bin/publish /opt/hello                                                                       0.0s
 => exporting to docker image format                                                                          2.1s
 => => exporting layers                                                                                       0.1s
 => => exporting manifest sha256:b1ae106290b3db89b17c874ff8899ea7047400b1fe82c1d75f8b6ce10c3d24ec             0.0s
 => => exporting config sha256:8e9bd99f2f9fea78ccaea5dabd817e06203d3709d28b26388edbb0b5fc141576               0.0s
 => => sending tarball                                                                                        2.0s
```

change pods/hello.yaml

```diff
  apiVersion: v1
  kind: Pod
  metadata:
    name: hello
    labels:
      app.kubernetes.io/name: 2545bb8b-e59c-4f4a-b362-2d1591215f25
  spec:
    containers:
      - name: app
-       image: hello:v0
+       image: hello:v1
```

kubectl apply --filename ./pods/hello.yaml --force
```
pod/hello configured
```


kubectl get pods
```
NAME    READY   STATUS    RESTARTS      AGE
hello   1/1     Running   1 (15s ago)   46m
```

forward the port in the ranchar desktop ui

GET http://localhost:5000

```
404e4879-8d29-4751-8f81-66e277ce977d
Hello.
```

define a second pod

```diff
📁 ..
  📁 .
    📁 applications
      📁 hello
        ✋ .gitignore
        🧾 appsettings.Development.json
        🧾 appsettings.json
        🐳 Containerfile
        🌐 hello.http
        🧾 Martin.Hello.csproj
        🧾 Program.cs
        🧾 publish-me-daddy.ps1
    📁 pods
+     🧾 hello-2.yaml
      🧾 hello.yaml
    📁 services
      🧾 hello.yaml
    📑 README.md
```

hello-2.yaml

```yaml
apiVersion: v1
kind: Pod
metadata:
  name: hello-2
  labels:
    app.kubernetes.io/name: 2545bb8b-e59c-4f4a-b362-2d1591215f25
spec:
  containers:
    - name: app
      image: hello:v1
```

note that `hello-2` is a duplicate of `hello`
it even has the same label, without which the service would not target it

kubectl apply --filename ./pods/hello-2.yaml
```
pod/hello-2 created
```


for ($i = 0; $i -lt 10; $i++) { Invoke-RestMethod -Uri http://localhost:5000 }
```
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
2a884b76-2fbc-4a74-923b-a3466286d056
Hello.
```

only one pod responds, the trafix isn't spread by the service among the two pods as it should, because we can't have nice things, let's try to delay the response of our application by introducing an artificial delay

Program.cs

```diff
  var builder = WebApplication.CreateBuilder(args);
  var app = builder.Build();
  var aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication = Guid.NewGuid().ToString();

- app.MapGet("/", () => $"{aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication}\nHello.");
+ app.MapGet("/", async () =>
+ {
+   await Task.Delay(500);
+   return $"{aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication}\nHello.";
+ });

- app.MapGet("/{name}", (string name) => $"{aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication}\nHello, {name}.");
+ app.MapGet("/{name}", async (string name) =>
+ {
+   await Task.Delay(500);
+   return $"{aUniqueIdThatIsSetOnlyOnceAtStartupOfTheApplication}\nHello, {name}.";
+ });

  app.Run();
```

add the image build to `publish-me-daddy.ps1`

```
+ param([String] $Tag)

  dotnet publish "$PSScriptRoot" --configuration Release --no-self-contained --output "$PSScriptRoot/bin/publish"
+ nerdctl image build --tag "hello:$Tag" --namespace "k8s.io" "$PSScriptRoot"
```

./applications/hello/publish-me-daddy.ps1 -Tag "v2"

```
MSBuild version 17.4.1+9a89d02ff for .NET
  Determining projects to restore...
  All projects are up-to-date for restore.
  Martin.Hello -> ...\applications\hello\bin\Release\net7.0\Martin.Hello.dll
  Martin.Hello -> ...\applications\hello\bin\publish\
[+] Building 4.6s (7/7)
[+] Building 4.7s (7/7) FINISHED
 => [internal] load build definition from Containerfile                                                       0.1s
 => => transferring dockerfile: 167B                                                                          0.0s
 => [internal] load .dockerignore                                                                             0.1s
 => => transferring context: 2B                                                                               0.0s
 => [internal] load metadata for mcr.microsoft.com/dotnet/aspnet:7.0                                          2.2s
 => [internal] load build context                                                                             0.3s
 => => transferring context: 183.12kB                                                                         0.2s
 => CACHED [1/2] FROM mcr.microsoft.com/dotnet/aspnet:7.0@sha256:54a3864f1c7dbb232982f61105aa18a59b976382a4e  0.0s
 => => resolve mcr.microsoft.com/dotnet/aspnet:7.0@sha256:54a3864f1c7dbb232982f61105aa18a59b976382a4e720fe18  0.0s
 => [2/2] COPY ./bin/publish /opt/hello                                                                       0.0s
 => exporting to docker image format                                                                          2.0s
 => => exporting layers                                                                                       0.1s
 => => exporting manifest sha256:5bfd400c905cab0a7e8556cd26e33340dc3ab006d1912935bec9a58835794daa             0.0s
 => => exporting config sha256:229dadd01e6ec31a1051254ee81b3d94b04793030715be871febf27627f5f518               0.0s
 => => sending tarball                                                                                        1.9s
Loaded image: docker.io/library/hello:v2
```

kubectl apply --recursive --filename ./pods --filename ./services --force
```
pod/hello-2 configured
pod/hello configured
service/hello unchanged
```

for ($i = 0; $i -lt 10; $i++) { Invoke-RestMethod -Uri http://localhost:5000 }
```
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
983433b6-1b9d-4f6d-b0c4-0b651a892ccd
Hello.
```

Same results, just for a longer waiting time

-->
