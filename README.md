# Semantic Driven Modeling

This repository contains all the code and documentation representing my idea of driving the modeling starting from the mode semantics. The code is very pragmatic and provide a valid proof of its applicability also in terms of performance.

I wrote and presented this **code** (src folder) and **documentation** (docs folder) as part of my Master Degree in Electronic Engineering in the [University of Genoa](https://corsi.unige.it/8732) on December 21, 2021. If you are wondering what meant for me getting a master degree in Engineering over 50 years of age, you can read my [heartfelt foreword](docs/Foreword.md).

The full thesis is also [available here](docs/SemanticDrivenModeling.pdf) and contains detailed documentation of the idea, the code, the challenges and the possible future outcomes.

---

Semantic Driven Modeling is a proposal to solve a very common problem in modern software architecture: versioning the public models of the services that makes up a distributed application.

Every distributed system is, by definition, composed by different components living in a separate process. The goal of the application's architecture is to provide the best possible orchestration of those components so that the application requirements can be fulfilled. Decoupling is the first step to fulfill a number of advantages such as scalability, reliability, availability, component reuse and easier maintenance.

Anyway, regardless the strategy chosen to decouple those components, the knowledge of the data structures exposed by a component inherently causes a dependency in the consumer components.  The friction caused by those dependencies resides in the public model changes that may occur over time.

Our goal is to provide the design strategy and tools that allows to freely evolve the public model used in a closed environment and to easily define the policies, rules and validators that ensure the model semantic correctness.

This thesis is structured in two sections. The first analyzes how the public model is affected by versioning in the distributed systems. The second exposes the proposed solution in both architectural and technical perspectives, concluding with an analysis on the practical outcomes that may derive from this work.

Raffaele