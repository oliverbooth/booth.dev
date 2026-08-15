<h1 align="center"><img src="icon.png"></h1>
<h1 align="center">booth.dev</h1>
<p align="center">
<img src="https://img.shields.io/gitlab/pipeline-status/oliver%2Fbooth.dev?gitlab_url=https%3A%2F%2Fgit.booth.dev%2F&branch=main&style=flat-square" alt="Gitlab Pipeline Status" title="Gitlab Pipeline Status">
<a href="https://github.com/oliverbooth/booth.dev/issues"><img src="https://img.shields.io/github/issues/oliverbooth/booth.dev?style=flat-square" alt="GitHub Issues" title="GitHub Issues"></a>
<a href="https://github.com/oliverbooth/booth.dev/blob/master/LICENSE.md"><img src="https://img.shields.io/github/license/oliverbooth/booth.dev?style=flat-square" alt="MIT License" title="MIT License"></a>
</p>

Source code for my website https://booth.dev.

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
For license details, please see the [LICENSE.md](LICENSE.md) file.
In short, this repository is made publicly available for **reference and educational purposes only**.
It is **not** an open-source license, and it does **not** grant permission to copy, redistribute, or republish this site or its contents in
whole or in part.

## Contact
For questions or support, feel free to reach out to me from my links on my [about page](https://booth.dev/about).
