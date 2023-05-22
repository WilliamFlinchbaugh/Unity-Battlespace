# Unity-Battlespace
Multi-Agent Reinforcement Learning in Simulated Aviation Battle Scenarios
 
# Contributors:
William Flinchbaugh - I did all of the work for this project up to this point
 
# Background of project:
CAE contacted UNT a while back about research with a deep reinforcement learning model that could help train pilots in aviation battle scenarios. The funding never went through, so the goal is to create a working model to pitch back to CAE. We want funding for a paper on the environment and model, or just general usage of MARL in this fashion. They are also potentially looking for interns.
 
# Status/Updates:
At the start of 2022, Rebecca and Mounika created a basic q-learning model with a relatively basic environment. The graphics were only still images from matplotlib.
In Summer 2022, I essentially completely transformed everything. I adapted the old environment to an [OpenAI Gym](https://github.com/openai/gym) environment that uses pygame. I trained the one agent using [Stable-Baselines3](https://stable-baselines3.readthedocs.io/en/master/) PPO and DQN. First we trained it against an agent using random choice (it had the choices forward, shoot, to enemy base, and to enemy plane). Then, I took that trained agent and placed it into the blue plane and trained the red plane against it.
 
However, we wanted to tackle multi-agent reinforcement learning (MARL) to see if we could get planes to collaborate. So, I turned that Gym environment into a [PettingZoo](https://pettingzoo.farama.org/) environment. We had to switch off of SB3 because it does not support MARL.
 
After using some completely decentralized algorithms, I transitioned to looking for strategies to harbor collaboration between teammates, but still have the planes make their own independent decisions. Here are the approaches I could find (there are not many of these approaches yet):
- [Learning to Share (LToS)](https://arxiv.org/pdf/2112.08702.pdf)
- [Actor-Attention-Critic (MAAC)](https://arxiv.org/pdf/1810.02912.pdf)
- [Multi-Agent Deep Deterministic Policy (MADDPG)](https://arxiv.org/abs/1706.02275)

I went with MADDPG because of the simplicity. I referenced two different repos for help on the model:
- https://github.com/shariqiqbal2810/maddpg-pytorch
- https://github.com/philtabor/Multi-Agent-Deep-Deterministic-Policy-Gradients
 
After implementing the MADDPG model, we branched off into two different ways of playing. One was the "self-play" approach where both teams were learning against each other, but this didn't give many results. Instead, I created an "instinct agent" or an algorithm for the opposing team that has a fixed policy. See more details below under behavior.

This repo is the implementation of what we had prior, but now in Unity, 3D, and using realistic flight physics. The setup is self-play and uses the [MA-POCA Algorithm](http://aaai-rlg.mlanctot.info/papers/AAAI22-RLG_paper_32.pdf) that is baked into Unity's ML-Agents framework. We have a fully-functioning environment and a model which is generally able to track down the target and kill it. The model and reward function definitely requires more refining, but training time/resources are very heavy, so further refining has been paused for now.

### MA-POCA
This is a cooperative MARL algorithm which tries to fix issues with the creation and destruction of agents.

I won't go into specifics of the algorithm, but the paper can be found [here](http://aaai-rlg.mlanctot.info/papers/AAAI22-RLG_paper_32.pdf)
   
# Installation Guide:
This project was built in Unity editor version 2021.3.16f1. The ML-Agents version used was 2.0-Verified and can be found [here](https://github.com/Unity-Technologies/ml-agents/tree/2.0-verified)

Just follow the installation instructions for ML-Agents Toolkit found [here](https://github.com/Unity-Technologies/ml-agents/blob/2.0-verified/docs/Installation.md)
 
The config file for training is included in the assets folder.

If you need any help getting the code to run, feel free to email me at WilliamFlinchbaugh@gmail.com or message me on discord: BallpointPen#6113
 
# Papers/Tutorials/Docs:
ML-Agents is not super widely used, so it can be difficult to find resources. I'll try to update this with different resources for understanding everything as I go.
