﻿"use client"
import {Autocomplete, Box, CircularProgress, Grid2, TextField, Typography} from "@mui/material";
import {useEffect, useRef, useState} from "react";
import {useRouter} from "next/navigation";
import {PokemonSpeciesDto} from "@shared/types";
import {capitalizeFirstLetter, padWithLeadingZeros} from "@shared/utils";
import {searchByName} from "@shared/api";

export default function SearchBar() {
    const router = useRouter();
    const inputRef = useRef<HTMLInputElement>(null);
    const [options, setOptions] = useState<PokemonSpeciesDto[]>([]);
    const [inputValue, setInputValue] = useState('');
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchData = async () => {
            try {
                if (inputValue.length > 1) {
                    const pokemons = await searchByName(inputValue);
                    if (pokemons) {
                        setOptions(pokemons);
                    }
                }
            } catch (error) {
                console.error('Error fetching data:', error);
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, [inputValue]);

    return (
        <Grid2 container
               direction="column"
               alignItems="center"
               mb={2}>
            <Grid2 size={{
                xs: 12,
                sm: 8
            }}>
                <Autocomplete
                    onChange={(_event, newValue: PokemonSpeciesDto | null) => {
                        if (newValue) {
                            if (inputRef.current) {
                                inputRef.current.blur()
                            }
                            router.push(`/pokemon/${newValue.name}`);
                        }
                    }}
                    onInputChange={(event, newInputValue) => {
                        setInputValue(newInputValue);
                    }}
                    noOptionsText="No Pokemons found"
                    loading={loading}
                    loadingText={<CircularProgress/>}
                    options={options}
                    filterOptions={(x) => x}
                    clearOnEscape
                    getOptionLabel={(option) => option.name}
                    renderOption={(props, option) => {
                        const {key, ...optionProps} = props;
                        return (
                            <Box
                                key={key}
                                component="li"
                                {...optionProps}
                            >
                                <Grid2 container
                                       textAlign={"center"}
                                       spacing={2}>
                                    <Grid2>
                                        <Typography color={"textSecondary"}>
                                            #{padWithLeadingZeros(option.id, 4)}
                                        </Typography>
                                    </Grid2>
                                    <Grid2>
                                        <Typography>
                                            {capitalizeFirstLetter(option.name)}
                                        </Typography>
                                    </Grid2>
                                </Grid2>
                            </Box>
                        )
                    }}
                    renderInput={(params) => (
                        <TextField
                            inputRef={inputRef}
                            {...params}
                            label="Search for Pokemons"
                            slotProps={{
                                htmlInput: {
                                    ...params.inputProps,
                                    autoComplete: 'new-password', // disable autocomplete and autofill
                                },
                            }}
                        />
                    )}
                />
            </Grid2>
        </Grid2>
    )
}