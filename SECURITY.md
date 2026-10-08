# Security

Please do not include Gemini API keys, Riot credentials, player chat logs, or screenshots in public issues.

RiftLingo sends only the user-selected chat-region screenshot to the Google Gemini API. Screenshots are held in memory and are not written to disk by the application. API keys are protected with Windows DPAPI for the current Windows account.

RiftLingo does not require administrator privileges. Report unexpected requests for elevation, network destinations other than the configured translation provider, or writes outside `%LOCALAPPDATA%\RiftLingo` as security issues.
