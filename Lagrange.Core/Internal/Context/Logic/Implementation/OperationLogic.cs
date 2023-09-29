using Lagrange.Core.Common.Entity;
using Lagrange.Core.Internal.Context.Attributes;
using Lagrange.Core.Internal.Event.Protocol.Action;
using Lagrange.Core.Internal.Event.Protocol.Message;
using Lagrange.Core.Internal.Event.Protocol.System;
using Lagrange.Core.Message;

namespace Lagrange.Core.Internal.Context.Logic.Implementation;

[BusinessLogic("OperationLogic", "Manage the user operation of the bot")]
internal class OperationLogic : LogicBase
{
    private const string Tag = nameof(OperationLogic);
    
    internal OperationLogic(ContextCollection collection) : base(collection) { }

    public async Task<List<string>> GetCookies(List<string> domains)
    {
        var fetchCookieEvent = FetchCookieEvent.Create(domains);
        var events = await Collection.Business.SendEvent(fetchCookieEvent);
        return events.Count != 0 ? ((FetchCookieEvent)events[0]).Cookies : new List<string>();
    }

    public async Task<List<BotFriend>> FetchFriends(bool refreshCache = false) => 
            await Collection.Business.CachingLogic.GetCachedFriends(refreshCache);

    public async Task<List<BotGroupMember>> FetchMembers(uint groupUin, bool refreshCache = false) => 
            await Collection.Business.CachingLogic.GetCachedMembers(groupUin, refreshCache);
    
    public async Task<List<BotGroup>> FetchGroups(bool refreshCache)
    {
        if (refreshCache) await Collection.Business.PushEvent(InfoSyncEvent.Create());

        return await Collection.Business.CachingLogic.GetCachedGroups();
    }

    public async Task<MessageResult> SendMessage(MessageChain chain)
    {
        var sendMessageEvent = SendMessageEvent.Create(chain);
        var events = await Collection.Business.SendEvent(sendMessageEvent);
        return ((SendMessageEvent)events[0]).MsgResult;
    }

    public async Task<bool> MuteGroupMember(uint groupUin, uint targetUin, uint duration)
    {
        string? uid = await Collection.Business.CachingLogic.ResolveUid(groupUin, targetUin);
        if (uid == null) return false;
        
        var muteGroupMemberEvent = GroupMuteMemberEvent.Create(groupUin, duration, uid);
        var events = await Collection.Business.SendEvent(muteGroupMemberEvent);
        return events.Count != 0 && ((GroupMuteMemberEvent)events[0]).ResultCode == 0;
    }
    
    public async Task<bool> MuteGroupGlobal(uint groupUin, bool isMute)
    {
        var muteGroupMemberEvent = GroupMuteGlobalEvent.Create(groupUin, isMute);
        var events = await Collection.Business.SendEvent(muteGroupMemberEvent);
        return events.Count != 0 && ((GroupMuteGlobalEvent)events[0]).ResultCode == 0;
    }
    
    public async Task<bool> KickGroupMember(uint groupUin, uint targetUin, bool rejectAddRequest)
    {
        string? uid = await Collection.Business.CachingLogic.ResolveUid(groupUin, targetUin);
        if (uid == null) return false;
        
        var muteGroupMemberEvent = GroupKickMemberEvent.Create(groupUin, uid, rejectAddRequest);
        var events = await Collection.Business.SendEvent(muteGroupMemberEvent);
        return events.Count != 0 && ((GroupKickMemberEvent)events[0]).ResultCode == 0;
    }
    
    public async Task<bool> SetGroupAdmin(uint groupUin, uint targetUin, bool isAdmin)
    {
        string? uid = await Collection.Business.CachingLogic.ResolveUid(groupUin, targetUin);
        if (uid == null) return false;
        
        var muteGroupMemberEvent = GroupSetAdminEvent.Create(groupUin, uid, isAdmin);
        var events = await Collection.Business.SendEvent(muteGroupMemberEvent);
        return events.Count != 0 && ((GroupSetAdminEvent)events[0]).ResultCode == 0;
    }
    
    public async Task<bool> RenameGroupMember(uint groupUin, uint targetUin, string targetName)
    {
        string? uid = await Collection.Business.CachingLogic.ResolveUid(groupUin, targetUin);
        if (uid == null) return false;

        var renameGroupEvent = RenameMemberEvent.Create(groupUin, uid, targetName);
        var events = await Collection.Business.SendEvent(renameGroupEvent);
        return events.Count != 0 && ((GroupSetAdminEvent)events[0]).ResultCode == 0;
    }

    public async Task<bool> GetHighwayAddress()
    {
        var highwayUrlEvent = HighwayUrlEvent.Create();
        var events = await Collection.Business.SendEvent(highwayUrlEvent);
        return events.Count != 0 && ((HighwayUrlEvent)events[0]).ResultCode == 0;
    }

    public async Task<bool> RecallGroupMessage(uint groupUin, MessageResult result)
    {
        if (result.Sequence == null) return false;
        
        var recallMessageEvent = RecallGroupMessageEvent.Create(groupUin, result.Sequence.Value);
        var events = await Collection.Business.SendEvent(recallMessageEvent);
        return events.Count != 0 && ((RecallGroupMessageEvent)events[0]).ResultCode == 0;
    }
    
    public async Task<bool> RecallGroupMessage(MessageChain chain)
    {
        if (chain.GroupUin == null) return false;
        
        var recallMessageEvent = RecallGroupMessageEvent.Create(chain.GroupUin.Value, chain.Sequence);
        var events = await Collection.Business.SendEvent(recallMessageEvent);
        return events.Count != 0 && ((RecallGroupMessageEvent)events[0]).ResultCode == 0;
    }

    public async Task<bool> RequestFriend(uint targetUin, string message, string question)
    {
        var requestFriendSearchEvent = RequestFriendSearchEvent.Create(targetUin);
        var searchEvents = await Collection.Business.SendEvent(requestFriendSearchEvent);
        if (searchEvents.Count == 0) return false;
        await Task.Delay(5000);
        
        var requestFriendSettingEvent = RequestFriendSettingEvent.Create(targetUin);
        var settingEvents = await Collection.Business.SendEvent(requestFriendSettingEvent);
        if (settingEvents.Count == 0) return false;
        
        var requestFriendEvent = RequestFriendEvent.Create(targetUin, message, question);
        var events = await Collection.Business.SendEvent(requestFriendEvent);
        return events.Count != 0 && ((RequestFriendEvent)events[0]).ResultCode == 0;
    }
    
    public async Task<string?> GetClientKey()
    {
        var clientKeyEvent = FetchClientKeyEvent.Create();
        var events = await Collection.Business.SendEvent(clientKeyEvent);
        if (events.Count == 0) return null;
        return ((FetchClientKeyEvent)events[0]).ClientKey;
    }
}