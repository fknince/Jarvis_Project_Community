import os
import asyncio
import sys
import json
from dotenv import load_dotenv
from langchain_openai import AzureChatOpenAI
from browser_use import Agent, Browser, BrowserConfig

from browser_use import Agent
from pydantic import SecretStr
import sys
import logging

sys.stdout.reconfigure(encoding='utf-8')
sys.stderr.reconfigure(encoding='utf-8')

logging.basicConfig(encoding='utf-8')

load_dotenv()

import json

def remove_unwanted_fields(json_string):
    data = json.loads(json_string)

    def clean_data(obj):
        if isinstance(obj, dict):
            obj.pop("screenshot", None)
            obj.pop("page_coordinates", None)
            obj.pop("interacted_element", None)
            for key in obj:
                clean_data(obj[key])
        elif isinstance(obj, list):
            for item in obj:
                clean_data(item)

    clean_data(data)
    return json.dumps(data, indent=4, ensure_ascii=False)


async def run_agent(task):
    azure_deployment = os.getenv("AZURE_DEPLOYMENT")
    api_version = os.getenv("AZURE_API_VERSION")
    azure_endpoint = os.getenv("AZURE_ENDPOINT")
    api_key = os.getenv("AZURE_SECRET_KEY")

    if not all([azure_deployment, api_version, azure_endpoint, api_key]):
        raise Exception("Azure configuration variables are missing! Please check your .env file.")

    llm = AzureChatOpenAI(
        azure_deployment=azure_deployment,
        api_version=api_version,
        azure_endpoint=azure_endpoint,
        api_key=SecretStr(api_key)
    )
    browser = Browser(
        config=BrowserConfig(
            chrome_instance_path=r"C:\Program Files\Google\Chrome\Application\chrome.exe",
            disable_security=True
        )
    )
    agent = Agent(task=task, llm=llm,generate_gif=False,use_vision=False)
    result =await agent.run()
    json_data = json.dumps(result.model_dump(), indent=4)
    json_data=remove_unwanted_fields(json_data)
    '''with open("test.json", "w", encoding="utf-8") as dosya:
        dosya.write(json_data)'''
    print(json_data)



if __name__ == "__main__":
    if len(sys.argv) > 1:
        task_input = sys.argv[1]
        asyncio.run(run_agent(task_input))
    else:
        raise Exception("Task input should not be empty.")
