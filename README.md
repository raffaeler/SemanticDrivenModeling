# Semantic Driven Modeling

Semantic Driven Modeling is a proposal to solve a very common problem in modern software architecture: versioning the public models of the services that makes up a distributed application.

Every distributed system is, by definition, composed by different components living in a separate process. The goal of the application's architecture is to provide the best possible orchestration of those components so that the application requirements can be fulfilled. Decoupling is the first step to fulfill a number of advantages such as scalability, reliability, availability, component reuse and easier maintenance.

Anyway, regardless the strategy chosen to decouple those components, the knowledge of the data structures exposed by a component inherently causes a dependency in the consumer components.  The friction caused by those dependencies resides in the public model changes that may occur over time.

Our goal is to provide the design strategy and tools that allows to freely evolve the public model used in a closed environment and to easily define the policies, rules and validators that ensure the model semantic correctness.

This thesis is structured in two sections. The first analyzes how the public model is affected by versioning in the distributed systems. The second exposes the proposed solution in both architectural and technical perspectives, concluding with an analysis on the practical outcomes that may derive from this work.

More technical details will follow.

Raffaele