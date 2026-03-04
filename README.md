# Context-aware Decision-making Framework for Trustworthy Service Evaluation in Social Internet of Things (SIoT) 

## Overview 
This project introduces a context-aware framework for trustworthy service evaluation in Social Internet of Things (SIoT) environments. By integrating device status, environment, task type, service quality, and social relationships, the Mutual Context-aware Trustworthy Service Evaluation (MCTSE) model enables devices to select reliable service providers and defend against malicious behaviors. Unlike traditional trust models that focus only on quality of service or social relations, this approach combines multiple contextual factors and feedback from past interactions, resulting in more accurate and resilient trust decisions in dynamic SIoT scenarios.

## Problem Definition
How can service-consuming and service-providing devices in SIoT environments reliably evaluate each other’s trustworthiness when trust depends on multiple contexts?

In the SIoT model:
- Devices belong to users connected through a social network.
- Devices act as service consumers (SC) or service providers (SP).
- Trustworthiness depends not only on past interactions and social relations, but also on:
  - device status (e.g., energy, capability),
  - environment (time and location), and
  - task type.

Existing trust models treat trust as either:
- non-contextual, or
- single-context (e.g., only service quality or task type).

Such models fail to:
- detect dishonest behavior consistently,
- resist reputation attacks (e.g., bad-mouthing, self-promotion),
- adapt to changing conditions.

The goal is to compute mutual, context-aware trust values that allow devices to select the most trustworthy partners for service transactions.

## Modules and Architecture
### Metrics of Contextual Trust Evaluation
The framework defines contextual trust metrics that capture both device capabilities and social relationships under varying contexts. Metrics are categorized as:
- **Independent metrics**: Context-aware QoS similarity (between expected and advertised QoS under specific contexts).
- **Dependent metrics**: Contextual social trust (friendship, community, device social relations, all under task context).
- **Contextual feedback**: Past interaction feedback, with variance-based weighting to penalize inconsistent/malicious behavior.

These metrics provide a multi-dimensional, context-aware foundation for robust trust evaluation, enabling effective differentiation between honest and dishonest devices and improving resilience against trust-related attacks.

### Mutual Context-aware Trustworthy Service Evaluation (MCTSE)
The model integrates:
1. **Context-aware QoS Similarity Trust (CQoSSTrust)**: Cosine similarity between expected and advertised QoS.
2. **Context-aware Social Similarity Trust (CSSTrust)**: Social similarity (friendship, community, device relations) under a task context.
3. **Contextual Feedback with Variance**: Past feedback, variance-based weighting to penalize malicious behavior.

Trust is computed bidirectionally (SC→SP and SP→SC) for fairness and robustness.

## Evaluation Metrics
- **Success Rate**: Percentage of times the model selects the most trustworthy service.
- **Trust Accuracy**: Agreement between computed trust and ground-truth satisfaction.
- **Attack Resilience**: Performance under malicious behaviors (BMA/BSA, SPA, OOA).

Compared against three baseline models: SOA (non-contextual), SubM (single-context, subjective), ObjM (single-context, objective).

## Simulation Settings and Performance Comparison
- 600 devices (300 SC, 300 SP), 200 users from a synthetic Facebook dataset.
- Devices are honest or dishonest (dishonest may perform BMA, BSA, SPA, OOA attacks).
- Ground-truth trust: honest [0.80, 0.85], dishonest [0.55, 0.60].

## Preprocessing Steps
- **Context Vector Construction**: Each device is a vector (status, environment, task).
- **Social Information Exchange**: Devices exchange friend/community lists.
- **Interaction History Tracking**: Contextual feedback and variance recorded.
- **Attack Modeling**: Trust-related attacks simulated.

## Results
- MCTSE outperforms baselines in success rate and resilience under attacks.
- Improves service selection accuracy by up to 14%.
- Effectively detects and penalizes malicious behavior.

## Conclusion
The MCTSE model for SIoT environments improves trust evaluation by incorporating device status, environment, task type, QoS similarity, social similarity, and feedback variance. It reliably differentiates honest and dishonest devices, resists attacks, and selects higher-quality services compared to existing approaches.

---

# Coding and Running the Project

## Prerequisites
- Windows OS
- .NET Framework (recommended: 4.7.2 or later)
- Visual Studio (2017 or later) or any compatible C# IDE

## Project Structure
- `SimSIoT.sln`: Visual Studio solution file
- `SimSIoT/`: Main project folder
  - `Program.cs`: Entry point
  - `Simulator.cs`: Main simulation logic and UI
  - `DomainObjects/`: Contains core classes (Device, User, Service, etc.)
  - `Properties/`: Project settings and resources
  - `SP.txt`, `SR.txt`, `User.txt`, etc.: Input/output data files

## How to Build
1. Open `SimSIoT.sln` in Visual Studio.
2. Restore any missing NuGet packages (if prompted).
3. Build the solution (Ctrl+Shift+B).


## Customization
- You can modify simulation parameters (number of users/devices, attack types, etc.) in `Simulator.cs`.
- To use different datasets, replace the input files (e.g., `facebook_combined.txt`).

## Notes
- The simulation uses synthetic data for social relationships and device behavior.
- All results are saved as text files for further analysis.
- For best results, use the recommended .NET and Visual Studio versions.


## Resource

[Maryam Khani, Yan Wang, Mehmet A. Orgun, and Feng Zhu. 2018. Context-Aware Trustworthy Service Evaluation in Social Internet of Things. In Service-Oriented Computing: 16th International Conference, ICSOC 2018, Hangzhou, China, November 12-15, 2018, Proceedings. Springer-Verlag, Berlin, Heidelberg, 129–145.](https://doi.org/10.1007/978-3-030-03596-9_9)


