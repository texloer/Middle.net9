/// <reference types="vite/client" />

// WebView2 类型声明
interface Window {
  chrome?: {
    webview?: {
      addEventListener(type: 'message', listener: (event: any) => void): void;
      removeEventListener(type: 'message', listener: (event: any) => void): void;
      postMessage(message: string): void;
    };
  };
}
