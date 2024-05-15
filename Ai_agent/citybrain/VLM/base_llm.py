"""LLM 模型提供者的基类."""
import abc
from typing import List, Dict, Tuple, Optional, Any

class LLMProvider(abc.ABC):
    """LLM 模型的接口."""

    @abc.abstractmethod
    def create_completion(
        self,
        messages: List[Dict[str, str]],
        model: str,
        temperature: float,
        stop_tokens: Optional[List[str]] = None,
    ) -> Tuple[str, Dict[str, int]]:
        """从文本消息（可能还包括编码图像）创建一个完成的输出."""
        pass

    @abc.abstractmethod
    async def create_completion_async(
        self,
        messages: List[Dict[str, str]],
        model: str,
        temperature: float,
        stop_tokens: Optional[List[str]] = None,
    ) -> Tuple[str, Dict[str, int]]:
        """从文本消息（可能还包括编码图像）创建一个完成的输出（异步版本）."""
        pass

    @abc.abstractmethod
    def init_provider(self, provider_cfg) -> None:
        """使用 JSON 配置初始化提供者."""
        pass

    @abc.abstractmethod
    def assemble_prompt(self, template_str: str = None, params: Dict[str, Any] = None) -> List[Dict[str, Any]]:
        """将参数组合成供提供者使用的适当方式."""
        pass
