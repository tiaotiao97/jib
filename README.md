![image](https://images4.c-ctrip.com/target/0zb6t120008gdy00uD281.png)


在devops流程里面 构建镜像是一个非常重要的过程，一般构建镜像是写dockerfile文件然后通过docker client来构建的image。

docker client 会先检查本地有没有image，如果没有帮你 从镜像仓库 pull 下来
然后解析你写的dockerfile构建新的image。


本文带你了解
- pull 命令 背后是怎么做的？
- build 命令 背后是怎么做的？


下篇文章带你解析：
- 如果我不用docker 我如何构建一个镜像？

我们以微软的aspnet2.2为基础构建一个aspnetcore项目的镜像为例子

mcr.microsoft.com/dotnet/core/aspnet:2.2

#### 根据基础镜像REGISTRY去获取mainfest信息

![image](https://images4.c-ctrip.com/target/0zb39120008gb281xDE60.png)

```
https://mcr.microsoft.com/v2/dotnet/core/aspnet/manifests/2.2
Accept:
application/vnd.oci.image.manifest.v1+json,application/vnd.docker.distribution.manifest.v2+json,application/vnd.docker.distribution.manifest.v1+json
```
获取到的内容如下：
```
{
    "schemaVersion": 2,
    "mediaType": "application/vnd.docker.distribution.manifest.v2+json",
    "config": {
        "mediaType": "application/vnd.docker.container.image.v1+json",
        "size": 4039,
        "digest": "sha256:e7e3b238011ce0f2b9350153535fe273caa01f0e7188d0b91f965b3802ddc600"
    },
    "layers": [
        {
            "mediaType": "application/vnd.docker.image.rootfs.diff.tar.gzip",
            "size": 22524609,
            "digest": "sha256:804555ee037604c40de144f9f8da0d826d38db82f15d74cded32790fe279a8f6"
        },
        {
            "mediaType": "application/vnd.docker.image.rootfs.diff.tar.gzip",
            "size": 17692725,
            "digest": "sha256:970251047358aea56ba6db6975b14ff12470b75de0c2477f4445240ddd727fd4"
        },
        {
            "mediaType": "application/vnd.docker.image.rootfs.diff.tar.gzip",
            "size": 2978257,
            "digest": "sha256:f3d4c41a4fd13f35c0b46f19a4e27845f4695163cc7174d908ff84836bbc2f5a"
        },
        {
            "mediaType": "application/vnd.docker.image.rootfs.diff.tar.gzip",
            "size": 62145592,
            "digest": "sha256:bd391c46585f9f8d84992bbaa9087189148c1601968eaaf097d5b3ed60840e5e"
        }
    ]
}

```

mainfest文件里面都是摘要(digest)记录
- config信息摘要 
- 每个layer的摘要 （上面的例子有4个）

#### 根据上面的config信息摘要获取config详情
![image](https://images4.c-ctrip.com/target/0zb6w120008gb32q3E923.png)
```
GET:https://mcr.microsoft.com/v2/dotnet/core/aspnet/blobs/sha256:e7e3b238011ce0f2b9350153535fe273caa01f0e7188d0b91f965b3802ddc600
```


```json

{
    "architecture": "amd64",
    "config": {
        "Hostname": "",
        "Domainname": "",
        "User": "",
        "AttachStdin": false,
        "AttachStdout": false,
        "AttachStderr": false,
        "Tty": false,
        "OpenStdin": false,
        "StdinOnce": false,
        "Env": [
            "PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin",
            "ASPNETCORE_URLS=http://+:80",
            "DOTNET_RUNNING_IN_CONTAINER=true",
            "ASPNETCORE_VERSION=2.2.8"
        ],
        "Cmd": [
            "bash"
        ],
        "ArgsEscaped": true,
        "Image": "sha256:5ecfe4016ac8e911a94aa601a675f7204e9ccab00cbb08e7067c184ad40f34e9",
        "Volumes": null,
        "WorkingDir": "",
        "Entrypoint": null,
        "OnBuild": null,
        "Labels": null
    },
    "container": "14196c2f9c327d41e26682d32c7c89c4e7c78aa32f8b7501a23192035a9f4844",
    "container_config": {
        "Hostname": "",
        "Domainname": "",
        "User": "",
        "AttachStdin": false,
        "AttachStdout": false,
        "AttachStderr": false,
        "Tty": false,
        "OpenStdin": false,
        "StdinOnce": false,
        "Env": [
            "PATH=/usr/local/sbin:/usr/local/bin:/usr/sbin:/usr/bin:/sbin:/bin",
            "ASPNETCORE_URLS=http://+:80",
            "DOTNET_RUNNING_IN_CONTAINER=true",
            "ASPNETCORE_VERSION=2.2.8"
        ],
        "Cmd": [
            "/bin/sh",
            "-c",
            "curl -SL --output aspnetcore.tar.gz https://dotnetcli.azureedge.net/dotnet/aspnetcore/Runtime/$ASPNETCORE_VERSION/aspnetcore-runtime-$ASPNETCORE_VERSION-linux-x64.tar.gz     && aspnetcore_sha512='954072376698be69acb7e277df2c243f931e10529def21dcbf9ce277609b30d462126bf8b8b3cab36476bec3d63a927b8e44e59e4d4cade23eef45956fba1ffd'     && echo \"$aspnetcore_sha512  aspnetcore.tar.gz\" | sha512sum -c -     && mkdir -p /usr/share/dotnet     && tar -zxf aspnetcore.tar.gz -C /usr/share/dotnet     && rm aspnetcore.tar.gz     && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet"
        ],
        "Image": "sha256:5ecfe4016ac8e911a94aa601a675f7204e9ccab00cbb08e7067c184ad40f34e9",
        "Volumes": null,
        "WorkingDir": "",
        "Entrypoint": null,
        "OnBuild": null,
        "Labels": null
    },
    "created": "2019-12-28T08:12:05.676492579Z",
    "docker_version": "3.0.8",
    "history": [
        {
            "created": "2019-12-28T04:23:47.4966447Z",
            "created_by": "/bin/sh -c #(nop) ADD file:90a2c81769a336bed3f731f44a385f2a65b0916f517a0b77c06c224579bf9a9a in / "
        },
        {
            "created": "2019-12-28T04:23:47.719507596Z",
            "created_by": "/bin/sh -c #(nop)  CMD [\"bash\"]",
            "empty_layer": true
        },
        {
            "created": "2019-12-28T08:11:05.607009582Z",
            "created_by": "/bin/sh -c apt-get update     && apt-get install -y --no-install-recommends         ca-certificates                 libc6         libgcc1         libgssapi-krb5-2         libicu57         liblttng-ust0         libssl1.0.2         libstdc++6         zlib1g     && rm -rf /var/lib/apt/lists/*"
        },
        {
            "created": "2019-12-28T08:11:07.64336022Z",
            "created_by": "/bin/sh -c #(nop)  ENV ASPNETCORE_URLS=http://+:80 DOTNET_RUNNING_IN_CONTAINER=true",
            "empty_layer": true
        },
        {
            "created": "2019-12-28T08:11:16.475068844Z",
            "created_by": "/bin/sh -c apt-get update     && apt-get install -y --no-install-recommends         curl     && rm -rf /var/lib/apt/lists/*"
        },
        {
            "created": "2019-12-28T08:11:43.814078508Z",
            "created_by": "/bin/sh -c #(nop)  ENV ASPNETCORE_VERSION=2.2.8",
            "empty_layer": true
        },
        {
            "created": "2019-12-28T08:12:05.676492579Z",
            "created_by": "/bin/sh -c curl -SL --output aspnetcore.tar.gz https://dotnetcli.azureedge.net/dotnet/aspnetcore/Runtime/$ASPNETCORE_VERSION/aspnetcore-runtime-$ASPNETCORE_VERSION-linux-x64.tar.gz     && aspnetcore_sha512='954072376698be69acb7e277df2c243f931e10529def21dcbf9ce277609b30d462126bf8b8b3cab36476bec3d63a927b8e44e59e4d4cade23eef45956fba1ffd'     && echo \"$aspnetcore_sha512  aspnetcore.tar.gz\" | sha512sum -c -     && mkdir -p /usr/share/dotnet     && tar -zxf aspnetcore.tar.gz -C /usr/share/dotnet     && rm aspnetcore.tar.gz     && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet"
        }
    ],
    "os": "linux",
    "rootfs": {
        "type": "layers",
        "diff_ids": [
            "sha256:814c70fdae62bc26c603bfae861f00fb1c77fc0b1ee8d565717846f4df24ae5d",
            "sha256:0cf75cb98eb2e0a82631d4aff71b40ba79ff7f83e0361f696875e592a1a4cefc",
            "sha256:15e45d99c92686fb1fd61a41431d8400d7a0e8381595d09d666b0809c4f5d993",
            "sha256:579a8f1d6a123f98095c0b1a1395079f7504391fd2a8bc529dede305a2072a36"
        ]
    }
}
```
#### 根据diff_ids里面去下载对应layers
下载完后对比摘要一致，确保镜像文件合法性

路径规则：
https://mcr.microsoft.com/v2/dotnet/core/aspnet/blobs/sha256:XXXXXX


![image](https://images4.c-ctrip.com/target/0zb6w120008gdwi4y9C82.png)

![image](https://images4.c-ctrip.com/target/0zb0v120008gdwpa683F0.png)

![image](https://images4.c-ctrip.com/target/0zb0g120008gdwyf809F3.png)

![image](https://images4.c-ctrip.com/target/0zb0v120008gdwiwd4E96.png)



#### 构建我们的镜像

在基础镜像的配置基础上加入我们的自定义配置
- Entrypoint
- Cmd
- Ports
- Environment
- ImageWorkingDirectory
- Volumes
- Labels


在基础镜像的所有的layers把我们要打包到镜像也做成一个layer



#### 生成的镜像tar包解压出来




![image](https://images4.c-ctrip.com/target/0zb67120008gdx6kkAB70.png)


多了一个 tar.gz文件，解压之后 就是我们打包放进去的文件



![image](https://images4.c-ctrip.com/target/0zb6n120008gdwy3y3A9A.png)


- 原来基础镜像有4个layer 加上我们的 共5个
- config.json
- manifest.json


##### manifest.json对比与基础镜像
![image](https://images4.c-ctrip.com/target/0zb1c120008ge0k7r9858.png)
	

##### config.json对比与基础镜像
![image](https://images4.c-ctrip.com/target/0zb6j120008ge0pc51F5F.png)


#### 我们来复习下构建镜像的过程
- 1. 根据镜像名称拉取mainfest
- 2. 根据mainfest拉取config
- 3. 根据config拉取layers
- 4. 下载各个layer
- 5. 修改到基础镜像的配置(config.json和mainfest.json)
- 6. 加入我们要加入的文件layer


#### 知道原理后本项目工具的目的就是自动来实现整个过程

支持以下镜像仓库作为基础镜像构建
- docker hub
- aliyun
- 腾讯云
