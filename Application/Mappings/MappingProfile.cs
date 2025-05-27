using Application.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Workspace, WorkspaceDto>();
        CreateMap<Note, NoteDto>();
    }
}