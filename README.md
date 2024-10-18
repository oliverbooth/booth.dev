<h1 align="center"><img src="icon.png"></h1>
<h1 align="center">oliverbooth.dev</h1>
<p align="center">
<a href="https://github.com/oliverbooth/oliverbooth.dev/actions/workflows/dotnet.yml"><img src="https://img.shields.io/github/actions/workflow/status/oliverbooth/oliverbooth.dev/dotnet.yml?style=flat-square" alt="GitHub Workflow Status" title="GitHub Workflow Status"></a>
<a href="https://github.com/oliverbooth/oliverbooth.dev/issues"><img src="https://img.shields.io/github/issues/oliverbooth/oliverbooth.dev?style=flat-square" alt="GitHub Issues" title="GitHub Issues"></a>
<a href="https://github.com/oliverbooth/oliverbooth.dev/blob/master/LICENSE.md"><img src="https://img.shields.io/github/license/oliverbooth/oliverbooth.dev?style=flat-square" alt="MIT License" title="MIT License"></a>
</p>

Source code for my website https://oliverbooth.dev.

## About
My site was formerly just a landing card which linked to various socials and two separate blogs: one code blog, one
non-code blog. These blogs were powered by.... *shudders* WordPress... Yes, I know.

I realised I needed to expand my website to include a portfolio as well as tutorials, and WordPress simply wasn't going
to cut it anymore.

Thus this project was born. This is a complete from-scratch rewrite of my website, now powered by ASP.NET Core. Almost
every component of the website is tailor-made by me, including my own makeshift blog CMS. That's right - every aspect of
my blog is now entirely custom, using an extensible Markdown renderer ([Markdig](https://github.com/xoofx/markdig))
supplemented with bodged integrations into [SmartFormat.NET](https://github.com/axuno/SmartFormat). This allowed me to
introduce Wikipedia-style templates and callouts, as well as rendering codeblocks exactly how I need them to
(using [Prism](https://prismjs.com/)).

## Contributing
Contributions are welcome, though I see seldom use for them as this is my personal and professional website tailored for
my specific requirements. However, I'm always happy to receive PRs for bug fixes and performance improvements, maybe
even new features. This, however, is entirely optional.

## License
This project is currently unlicensed until I decide on a license that is fit for this repository. This means, for the
time-being, that the rights to all assets and code are reserved. You may use the code here for educational purposes, but
directly copying my website for your own purposes is strictly forbidden at this current time.

## Contact
For questions or support, feel free to each out to me at https://oliverbooth.dev/contact.
