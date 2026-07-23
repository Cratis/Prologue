# Sample

Add a folder called Samples and within it we want to create a CRUD based system called Library, written in C#, leveraging standard out-of-the-box ASP.NET and EntityFramework (No Cratis Constructs).
Its capabilities is documented here: /Volumes/Code/Cratis/Studio/Source/LLM/test-prompt.md
We've also created an imaginary result of it here: /Volumes/Code/Cratis/Studio/Prologue/library-system. We do not need to match the results in this, just use it as inspiration.

It should be possible to configure it to use MSSQL and PostgreSQL.
In the Library folder, add a Core project and a Composition project and Service Defaults. The composition project should be an Aspire project.
The project should have a frontend for all the functionality. And it should be possible to trigger the simulation from the Aspire dashboard by custom actions on the Aspire dashboard for the Core service.
Testing project is in its own project. I would like to actually see 2 types of frontends; Razor based - typical server rendering and a TypeScript, React, Vite based. They behave differently.
They should look exactly the same and the Playwright tests should be capable of doing both, driven from the frontend.

It should be configured with open telemetry and have traces for controller actions (post and get).

The composition should be configured to be running the Extractor to be able to extract data from a running system and have the Receiver running and store the result
to Storage - MongoDB.

To simulate a system under use, we should have first of all some seed data, then it should simulate using Aspire Testing (https://aspire.dev/testing/overview/)
and Playwright (C#). Create scenarios that can be run to simulate a system with a lot of data (at least 10000 transactions, but ideally configurable - interactive when clicking the button)
being ingested and behavior happening, so that we can simulate what would effectively be a real system and then see that the Extractor is doing its thing.

The extractor acts as a reverse proxy for HTTP traffic, which means we need to be able to redirect traffic.
The extractor needs to be monitoring the database for transactions.
The extractor needs to be able to capture OTEL telemetry such as traces, metrics and potentially logs.

The extractor should be configured with a prologue Id and all the configuration it needs to be able to run.

Choice of database should be configurable from command line, if none specified - use PostgreSQL.

The Aspire setup should just add the projects in this repository as project references, not Docker images.

Fix anything that is stale, missing or is wrong. Read the README of the project to understand what this project does.
Fix any contradictions. For keys, favor using integers - as I think a lot of legacy systems does this with incremental keys.
