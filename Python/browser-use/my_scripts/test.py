import asyncio
from agent_runner import run_agent

async def main():
    task_input = "Open YouTube and search for the channel 'abi bunu robotla yaparız' and then get how many subscribers that has."
    await run_agent(task_input)

if __name__ == "__main__":
    asyncio.run(main())
