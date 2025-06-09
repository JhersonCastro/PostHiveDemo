<!-- Improved compatibility of back to top link: See: https://github.com/JhersonCastro/PostHiveDemo/pull/73 -->
<a id="readme-top"></a>
<!--
*** Thanks for checking out the Best-README-Template. If you have a suggestion
*** that would make this better, please fork the repo and create a pull request
*** or simply open an issue with the tag "enhancement".
*** Don't forget to give the project a star!
*** Thanks again! Now go create something AMAZING! :D
-->



<!-- PROJECT SHIELDS -->
<!--
*** I'm using markdown "reference style" links for readability.
*** Reference links are enclosed in brackets [ ] instead of parentheses ( ).
*** See the bottom of this document for the declaration of the reference variables
*** for contributors-url, forks-url, etc. This is an optional, concise syntax you may use.
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]



<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/JhersonCastro/PostHiveDemo/">
    <img src="https://github.com/JhersonCastro/PostHiveDemo/blob/master/PostHive/wwwroot/favicon.jpg" alt="Logo" width="80" height="80">
  </a>

  <h3 align="center">PostHive</h3>

  <p align="center">
    A Social Network for dummies!
    <br />
    <br />
    <br />
    <a href="http://posthive.tryasp.net/">View Demo</a>
    &middot;
    <a href="https://github.com/JhersonCastro/PostHiveDemo/issues/new?labels=bug&template=bug-report---.md">Report Bug</a>
    &middot;
    <a href="https://github.com/JhersonCastro/PostHiveDemo/issues/new?labels=enhancement&template=feature-request---.md">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#license">License</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

[![Product Name Screen Shot][product-screenshot]](http://posthive.tryasp.net/)

PostHive is a social platform prototype designed for university purposes, aimed at facilitating the creation and interaction with user-generated content. The project provides a structured, simple, and safe environment for users to express their ideas through posts and comments.

The platform is designed to be open, intuitive, and responsive, allowing users to interact with others in a shared space that prioritizes accessibility and ethical digital practices.

### General Objective

To design and implement an interactive web platform where users can register, publish content, and engage with others through comments, promoting a respectful community through basic behavioral guidelines.

### Specific Objectives

- Develop a secure and functional authentication system using **bcrypt**
- Implement features to create, edit, and delete public posts
- Create a comment system associated with each post
- Design a responsive and user-friendly interface for both mobile and desktop devices
- Apply best practices in security and accessibility throughout the platform

### Project Scope

**Included:**
- User registration and login system
- Content creation, editing, and visualization
- Comment functionality on posts
- Responsive UI using MudBlazor and Bootstrap

**Not included:**
- Administrative moderation tools
- User reports or sanctions
- Private messaging system
- Social media integration
- Monetization or advertisements

### Core Features

- 🔐 **Authentication:** Register, log in, and recover accounts using secure bcrypt-hashed passwords  
- 📝 **Posts:** Create, edit, delete, and view user-generated content  
- 💬 **Comments:** Comment on posts with simple interaction features  
- 📱 **Responsive Design:** Optimized layout using MudBlazor and Bootstrap  
- 💾 **Database:** SQL Server managed with Entity Framework

### System Architecture

The platform follows a layered architecture:

- **Frontend:** Developed using Blazor WebAssembly with MudBlazor for UI components and Bootstrap for additional styling
- **Backend:** Built with C# using Blazor Server or WebAssembly features
- **Database:** Relational model implemented using SQL Server, accessed through Entity Framework Core
- **Security:** Authentication handled via bcrypt, with proper input validation and secure data handling practices

### Technologies Used

| Component       | Technology                      |
|----------------|----------------------------------|
| Frontend        | Blazor + MudBlazor + Bootstrap  |
| Backend         | C# with Blazor                  |
| Database        | SQL Server + Entity Framework   |
| Authentication  | bcrypt                          |

### Ethical and Security Considerations

The system promotes core digital ethics: inclusion, respect, and responsible freedom of expression. Although there is no active moderation system, it is assumed that users will adhere to basic community norms. Personal data is protected through secure development practices and privacy-oriented design.

### Conclusion

PostHive aims to be a practical demonstration of modern web development using Blazor and C#, focusing on user interaction, ethical responsibility, and clean architectural design. The project serves as a functional educational tool, highlighting the creation of a social platform with real-world patterns, adapted to academic requirements and constraints.

<p align="right">(<a href="#readme-top">back to top</a>)</p>





### Built With

* [![Next][Blazor]][Blazor-url]
* [![React][JavaScript]][JavaScript-url]
* [![Bootstrap][Bootstrap.com]][Bootstrap-url]

<p align="right">(<a href="#readme-top">back to top</a>)</p>



<!-- GETTING STARTED -->
## Getting Started

This is an example of how you may give instructions on setting up your project locally.
To get a local copy up and running follow these simple example steps.

### Prerequisites

-Visual Studio 2022
-ASP.NET and Web Development workload

### Installation

_Below is an example of how you can instruct your audience on installing and setting up your app. This template doesn't rely on any external dependencies or services._

1. Clone the repo
   ```sh
   git clone https://github.com/JhersonCastro/PostHiveDemo.git
   ```
2. Enter your localhost:port in `const.cs`
   ```js
   const url = 'ENTER YOUR LOCALHOST:PORT';
   ```
3. Run and Enjoy!

<p align="right">(<a href="#readme-top">back to top</a>)</p>





<!-- LICENSE -->
## License

Distributed under the Unlicense License. See `LICENSE.txt` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>





<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/JhersonCastro/PostHiveDemo?style=for-the-badge
[contributors-url]: https://github.com/JhersonCastro/PostHiveDemo/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/JhersonCastro/PostHiveDemo?style=for-the-badge
[forks-url]: https://github.com/JhersonCastro/PostHiveDemo/network/members
[stars-shield]: https://img.shields.io/github/stars/JhersonCastro/PostHiveDemo?style=for-the-badge
[stars-url]: https://github.com/JhersonCastro/PostHiveDemo/stargazers
[issues-shield]: https://img.shields.io/github/issues/JhersonCastro/PostHiveDemo?style=for-the-badge
[issues-url]: https://github.com/JhersonCastro/PostHiveDemo/issues
[license-shield]: https://img.shields.io/github/license/JhersonCastro/PostHiveDemo?style=for-the-badge
[license-url]: https://github.com/JhersonCastro/PostHiveDemo/blob/master/LICENSE.txt
[product-screenshot]: https://github.com/JhersonCastro/PostHiveDemo/blob/master/Screenshot%202025-06-08%20120602.png
[Blazor]: https://img.shields.io/badge/Blazor-20232A?style=for-the-badge&logo=blazor&logoColor=61DAFB
[Blazor-url]: https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor
[JavaScript]: https://img.shields.io/badge/JavaScript-20232A?style=for-the-badge&logo=JavaScript&logoColor=#F7DF1E
[JavaScript-url]: https://developer.mozilla.org/en-US/docs/Web/JavaScript
[Bootstrap.com]: https://img.shields.io/badge/Bootstrap-563D7C?style=for-the-badge&logo=bootstrap&logoColor=white
[Bootstrap-url]: https://getbootstrap.com
