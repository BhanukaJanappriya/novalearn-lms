import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useState } from "react";
import { apiClient } from "@/services/apiClient";
import type { PostLinkInput, UploadInput, WallFilters, WallResource } from "./types";

export const wallKeys = {
  all: ["wall"] as const,
  list: (filters: WallFilters) => [...wallKeys.all, filters] as const,
};

export function useWall(filters: WallFilters) {
  return useQuery({
    queryKey: wallKeys.list(filters),
    queryFn: async () => {
      const { data } = await apiClient.get<WallResource[]>("/resources", {
        params: {
          courseId: filters.courseId || undefined,
          kind: filters.kind || undefined,
          search: filters.search || undefined,
        },
      });
      return data;
    },
    staleTime: 15_000,
  });
}

export function usePostLink() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (input: PostLinkInput) => {
      const { data } = await apiClient.post<WallResource>("/resources/links", input);
      return data;
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: wallKeys.all }),
  });
}

/**
 * Uploads a file, reporting progress so a large video does not look like a hung button.
 *
 * The client's default JSON content type is not overridden here on purpose: axios drops it for a
 * FormData body so the browser can set multipart with the boundary marker only it knows. Setting
 * one by hand is how uploads end up with a boundary the server cannot parse.
 */
export function useUploadResource() {
  const queryClient = useQueryClient();
  const [progress, setProgress] = useState(0);

  const mutation = useMutation({
    mutationFn: async (input: UploadInput) => {
      const form = new FormData();
      form.append("file", input.file);
      form.append("title", input.title);
      if (input.description) form.append("description", input.description);
      if (input.courseId) form.append("courseId", input.courseId);

      setProgress(0);

      const { data } = await apiClient.post<WallResource>("/resources/uploads", form, {
        onUploadProgress: (event) => {
          if (event.total) {
            setProgress(Math.round((event.loaded / event.total) * 100));
          }
        },
      });

      return data;
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: wallKeys.all }),
  });

  return { ...mutation, progress };
}

export function useDeleteResource() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/resources/${id}`);
    },
    onSuccess: () => queryClient.invalidateQueries({ queryKey: wallKeys.all }),
  });
}
