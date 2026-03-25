# Contributing to EcoKnow Creator

Thank you for your interest in contributing to EcoKnow Creator! This document outlines the different ways you can contribute and the processes we follow.

## 3 Ways to Contribute

### 1. Playing Scenarios (Data Contributions) [Easy]

By playing scenarios you generate valuable data that helps improve the simulation.

TODO: Where should scenario data exports be sent?

TODO: Agree next steps for scenario data submission pipeline.

### 2. Creating Scenarios [Medium]

You can contribute by building new scenarios for the community to play.

TODO: What documentation is needed for scenario creation guides? Where will it live? I've been holding off since I want it to be on a GitHub wiki, but we can't access that until the project goes open source.

TODO: Who will produce video content / guides?

### 3. Source Code Contributions [Hard]

This is the primary focus of this guide. We follow a structured process to keep the codebase stable and collaborative.

## Source Code Contribution Process

### Overview

All source code contributions go through:

**Issue → Branch → Pull Request → Review → Merge**.

### Step-by-Step

1. **Open or claim an Issue** - Before writing any code, ensure there is a GitHub Issue describing the work. If one doesn't exist, create it and wait for discussion/approval from the team.
2. **Create a relevant branch** - Branch from `main` using the naming convention: `feat/<issue-number>-short-description` (e.g. `feat/21-environment-zones`).

Full branchlist:
* bugfix: use 'fix'
* feature: use 'feat'
* spike: clone repo to your own github

3. **Develop on your branch** - Make your changes, committing regularly with clear messages.
4. **Open a Pull Request** - When your work is ready, open a PR against `main`. Reference the issue number in the PR description.
5. **Code Review** - At least one other contributor must review and approve the PR before it can be merged.
6. **Merge** - Once approved, the PR is merged into `main`.

### Decisions

TODO: When is an issue required? (all changes, or only features/bugs?)

TODO: Branch naming conventions - confirm or revise `feat/<issue-number>-short-description`.

TODO: PR expectations - what makes a good PR? (description, screenshots, testing notes?)

TODO: Who reviews? How many approvals are required before merge?

## Spike Work Policy

We definitely want to encourage exploratory, experimental work - we call this **spike** work. Spikes are a great way to prototype ideas and test feasibility. However:

- Spike work should happen on your own fork, in your own private github.
- Spike code **must not** be committed directly to the source repository.
- Instead, spikes should **inform feature proposals** that are then discussed with the wider team via a GitHub Issue before any implementation begins.

If you've done spike work that you think should become a feature, open an issue describing what you learned and proposing the change. The team will discuss it and, if agreed, it will follow the standard contribution process above.



### Decisions

TODO: What counts as spike work?

TODO: Where should spike work live? (forks?)

TODO: How does a spike become a feature proposal?

TODO: How big should a feature request be?

TODO: How do we make sure people feel spikes are valued, not discouraged?

## Project Areas and Ownership

The codebase is organised into several key areas. When contributing, it helps to know which area your change falls under:

### Calculators (Simulation)

The mathematical models and logic that determine how entities interact with each other during the simulation. Changes here affect the core simulation behaviour.

### Sandbox (Visualisation)

Everything the player sees and interacts with during a session - graphics, colours, UI/UX, icons, and visual feedback. This covers the runtime experience of playing a scenario.

### Scenario Editor

The tooling used to construct scenarios. We are working towards a node-based versioning system for scenario construction. Contributions in this area should be mindful of that direction.

### Map Editor

The Map Editor is a vital part of the EcoKnow Creator project, but is maintained on it's own [GitHub page](https://github.com/EcoKnowGames/map).

The Map Editor is responsible for drawing map layouts, zoning areas and then populating them with entities.

### Decisions

TODO: Should specific people own or be responsible for each area?

TODO: Define ownership/responsibility for: Calculators, Sandbox, Scenario Editor, Map Editor.

## Versioning and Releases

TODO: Versioning scheme (e.g. semantic versioning). Right now we have no versioning.

TODO: What constitutes a release? E.g. how should we but moving things into a user facing stable build, what Scenarios does it contain?

TODO: Release cadence - fixed schedule vs. feature-driven?

TODO: Who is responsible for building releases?

TODO: How do releases relate to branches? (e.g. release branches, tags)

TODO: Changelog and release notes process.

## Getting Started

1. Fork the repository at [github.com/EcoKnowGames/creator](https://github.com/EcoKnowGames/creator).
2. Clone your fork locally.
3. Open the project in Unity.
4. Create a branch for your work.
5. When ready, push your branch and open a Pull Request.

## Questions?

If you're unsure about anything, open a GitHub Issue to start a discussion.